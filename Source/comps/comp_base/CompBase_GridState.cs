using System;
using System.Text;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

// ReSharper disable once InconsistentNaming
public abstract class CompBase_GridState<T, TP> : CompBase_Grid<TP>
    where TP : CompProperties
    where T : CompBase_GridState<T, TP>
{
    protected static readonly StateIdle Idle = new();
    protected string Error;

    private State _currentState;

    public int progress;

    public State CurrentState
    {
        get => _currentState ??= Idle;
        set
        {
            if (value != _currentState)
                Error = default;
            _currentState = value;
            progress = 0;
        }
    }

    public override void PostExposeData()
    {
        if (ScribeState(ref _currentState, "currentState"))
        {
            Scribe_Values.Look(ref progress, "progress");
        }
    }

    public bool ScribeState(ref State state, string name)
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            if (state != default && state != Idle)
            {
                var stateClass = state.GetType().FullName;
                var stateTicks = state.Ticks;
                Scribe_Values.Look(ref stateClass, $"{name}Name");
                Scribe_Values.Look(ref stateTicks, $"{name}Ticks");
                state.ExposeData();
                return true;
            }
        }
        else if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            string stateClass = default;
            var stateTicks = 0;
            Scribe_Values.Look(ref stateClass, $"{name}Name");
            Scribe_Values.Look(ref stateTicks, $"{name}Ticks");
            if (stateClass != default)
            {
                try
                {
                    state = Activator.CreateInstance(Type.GetType(stateClass)!, stateTicks) as State;
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error($"ArchInf: Can't load current state for {nameof(GetType)}:\n{e.Message}");
                    state = Idle;
                }
            }
        }

        return false;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (Power is null) throw new Exception($"ArchInf: {GetType().FullName} can't work without {nameof(CompPowerTrader)}");
        if (Member is null) throw new Exception($"ArchInf: {GetType().FullName} can't work without {nameof(GridMemberComp)}");
    }

    protected AcceptanceReport Message(string message, bool silent)
    {
        if (!silent) Messages.Message(message, parent, MessageTypeDefOf.RejectInput);
        return message;
    }

    public abstract AcceptanceReport TryRun(bool silent = false);

    public virtual bool OnComplete() => false;

    protected virtual AcceptanceReport Stop(string reason, bool silent)
    {
        CurrentState = Idle;

        if (reason != default)
        {
            if (silent)
                Error = reason;
            else
                Messages.Message(reason, parent, MessageTypeDefOf.RejectInput);
        }

        return reason;
    }

    public override void CompTick()
    {
        // Does nothing on idle
        if (CurrentState == Idle) return;
        if (!Power.PowerOn) return;

        if (progress >= CurrentState.Ticks)
        {
            if (parent.IsHashIntervalTick(50))
                CurrentState.OnProgressComplete((T)this);
            return;
        }

        if (progress == 1) CurrentState.OnFirstTick((T)this);
        CurrentState.OnCompTick((T)this);

        progress = Math.Min(++progress, CurrentState.Ticks);
    }

    public override string CompInspectStringExtra()
    {
        if (!Power.PowerOn)
            return "JAI.Error.PoweredOff".Translate();

        if (Error != default)
            return Error;

        var sb = new StringBuilder();
        CurrentState.CompInspectStringExtra(sb, (T)this);
        return sb.TrimEnd().ToString();
    }

    public abstract class State(int ticks) : IExposable
    {
        public abstract string Label { get; }
        public virtual string Color => "green";
        public virtual bool ShowProgress => true;
        public int Ticks = ticks;

        public virtual void OnProgressComplete(T owner)
        {
        }

        public virtual void OnCompTick(T owner)
        {
        }

        public virtual void OnFirstTick(T owner)
        {
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref Ticks, "ticks");
        }

        public virtual void CompInspectStringExtra(StringBuilder sb, T owner)
        {
            var progress = (float)owner.progress / Ticks * 100f;
            sb.Append("JAI.State".Translate()).Append(": <color=").Append(Color).Append(">").Append(Label).Append("</color>");
            if (ShowProgress)
                sb.Append(", ").Append(progress.ToString("0.")).AppendLine("%");
            else
                sb.AppendLine();
        }
    }

    protected class StateIdle(int ticks = int.MaxValue) : State(ticks)
    {
        public override string Label => "JAI.State.Idle".Translate();
        public override string Color => "white";
        public override bool ShowProgress => false;
    }
}
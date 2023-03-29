using System;
using System.Text;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable once InconsistentNaming
public abstract class CompBase_Stageable<T, TP> : CompBase_Membered<TP> where TP : CompProperties
{
    protected static readonly StateIdle Idle = new();

    private State _currentState;

    public int progress;

    public State CurrentState
    {
        get => _currentState ??= Idle;
        set
        {
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

    public virtual AcceptanceReport CanStart(bool silent = false)
    {
        return false;
    }

    public abstract AcceptanceReport StartAction(bool silent = false);

    protected virtual void StopAction(string reason = default)
    {
        if (CurrentState == Idle) return;
        CurrentState = Idle;
        if (reason != default) Message(reason, false);
    }

    public virtual bool FinalAction()
    {
        return false;
    }

    public override void CompTick()
    {
        // Does nothing on idle
        if (CurrentState == Idle) return;
        if (!Power.PowerOn) return;

        if (progress >= CurrentState.Ticks)
        {
            if (parent.IsHashIntervalTick(50)) CurrentState.OnProgressComplete((T)(object)this);
            return;
        }

        if (progress == 1) CurrentState.OnFirstTick((T)(object)this);
        CurrentState.OnCompTick((T)(object)this);

        progress = Math.Min(++progress, CurrentState.Ticks);
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        if (Power.PowerOn && CurrentState != Idle)
        {
            var canContinue = CurrentState.CompInspectStringExtra(sb, (T)(object)this);
            if (canContinue)
            {
                sb.AppendLine($"Progress: {((float)progress / CurrentState.Ticks * 100f):0.}%");
            }
        }

        return sb.TrimEnd().ToString();
    }

    public abstract class State : IExposable
    {
        public abstract string Label { get; }
        public int Ticks;

        protected State(int ticks)
        {
            Ticks = ticks;
        }

        public virtual void OnProgressComplete(T owner)
        {
        }

        public virtual void OnCompTick(T owner)
        {
        }

        public virtual void OnFirstTick(T owner)
        {
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Ticks, "ticks");
        }

        public virtual bool CompInspectStringExtra(StringBuilder sb, T owner)
        {
            sb.AppendLine($"State: {Label}");
            return true;
        }
    }

    protected class StateIdle : State
    {
        public override string Label => "Idle".Translate();

        public StateIdle(int ticks = int.MaxValue) : base(ticks)
        {
        }
    }
}
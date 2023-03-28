using System;
using Verse;

namespace ArchotechInfusions.comps;

public class CompTest : ThingComp
{
    public CompPropsTest Props => props as CompPropsTest;

    private int _progress;
    private State _currentState;

    public override void PostExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            if (_currentState != State.Idle)
            {
                var stateClass = _currentState.GetType().FullName;
                var stateTicks = _currentState.Ticks;
                Scribe_Values.Look(ref stateClass, "state_class");
                Scribe_Values.Look(ref stateTicks, "state_ticks");
                Scribe_Values.Look(ref _progress, "progress");
            }
        }
        else if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            string stateClass = default;
            int stateTicks = default;
            Scribe_Values.Look(ref stateClass, "state_class");
            Scribe_Values.Look(ref stateTicks, "state_ticks");
            if (stateClass != default)
            {
                try
                {
                    _currentState = Activator.CreateInstance(Type.GetType(stateClass)!, this, stateTicks) as State;
                    Scribe_Values.Look(ref _progress, "progress");
                }
                catch (Exception e)
                {
                    Log.Error($"ArchInf: Can't load current state for {nameof(GetType)}: {e.Message}");
                    _currentState = State.Idle;
                    _progress = 0;
                }
            }
        }
    }

    public virtual void StopAction()
    {
        _currentState = State.Idle;
        _progress = 0;
    }

    public virtual void FinalAction()
    {
    }

    public override void CompTick()
    {
        // Does nothing on idle
        if (_currentState == State.Idle) return;

        if (_progress >= _currentState.Ticks)
        {
            if (parent.IsHashIntervalTick(50))
            {
                _currentState.OnProgressComplete();
            }

            return;
        }

        _progress++;
    }
}

public class CompPropsTest : CompProperties
{
    public int TicksFirst;
    public int TicksSecond;
    public IntRange TicksThird;

    public CompPropsTest() => compClass = typeof(CompTest);
}

public abstract class State
{
    public static readonly StateIdle Idle = new();

    public readonly string Label;
    public readonly CompTest Owner;
    public readonly int Ticks;

    public State(string label, CompTest owner, int ticks)
    {
        Label = label;
        Owner = owner;
        Ticks = ticks;
    }

    public virtual void OnProgressComplete()
    {
    }

    public virtual void OnCompTick()
    {
    }

    public virtual void OnFirstTick()
    {
    }
}

public class StateIdle : State
{
    public StateIdle() : base("Idle", default, int.MaxValue)
    {
    }

    public override void OnProgressComplete()
    {
    }
}

public class StateRecharging : State
{
    public StateRecharging(CompTest owner, int ticks) : base("Recharging", owner, ticks)
    {
    }

    public override void OnProgressComplete()
    {
    }

    public override void OnCompTick()
    {
    }
}

public class StateTransmission : State
{
    public StateTransmission(CompTest owner, int ticks) : base("Transmission", owner, ticks)
    {
    }

    public override void OnProgressComplete()
    {
    }
}

public class StateReceiving : State
{
    public StateReceiving(CompTest owner, int ticks) : base("Receiving", owner, ticks)
    {
    }

    public override void OnProgressComplete()
    {
    }

    public override void OnCompTick()
    {
    }
}
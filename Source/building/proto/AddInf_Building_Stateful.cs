using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.building.proto;

public abstract class AddInf_Building_Stateful : AddInf_Building
{
    protected const int StateIdle = 0;

    protected int State = StateIdle;
    protected AcceptanceReport StateError;
    protected int Ticks;
    protected int TicksInitial;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref State, nameof(State));
        Scribe_Values.Look(ref Ticks, nameof(Ticks));
        Scribe_Values.Look(ref TicksInitial, nameof(TicksInitial));
    }

    public override void Tick()
    {
        base.Tick();
        if (State == StateIdle) return;
        if (Ticks == 0) return;
        if (--Ticks == 0)
        {
            OnStateComplete();
            return;
        }

        OnStateTick();
    }

    protected void SetState(int state, int ticks = 0)
    {
        State = state;
        Ticks = ticks;
        TicksInitial = ticks;
    }

    protected void ResetState(AcceptanceReport reason = default)
    {
        SetState(StateIdle);
        StateError = reason;
    }

    protected abstract void OnStateTick();
    protected abstract void OnStateComplete();


    protected override void FillInspectStringExtra(StringBuilder sb)
    {
        if (DebugSettings.ShowDevGizmos)
            sb.AppendLine($"DEV: State: {State}, Ticks: {Ticks}, TicksInitial: {TicksInitial}");
        GetStateDescription(sb);
        GetStateInspectString(sb);
        if (StateError != default && !StateError.Accepted)
            sb.AppendLine(StateError.Reason);
    }

    protected virtual void GetStateDescription(StringBuilder sb)
    {
        if (State == StateIdle)
            sb.AppendLine("JAI.State.Idle".Translate());
    }

    protected virtual void GetStateInspectString(StringBuilder sb)
    {
    }

    protected AcceptanceReport Message(AcceptanceReport message, bool silent)
    {
        if (DebugSettings.ShowDevGizmos) Log.Warning($"JAI {LabelCap} Message ({silent}): {message.ToString()}");
        if (!silent && message != default && !message.Accepted)
            Messages.Message(message.Reason, this, MessageTypeDefOf.RejectInput);
        return message;
    }

    protected AcceptanceReport Stop(AcceptanceReport reason, bool silent)
    {
        if (DebugSettings.ShowDevGizmos) Log.Warning($"JAI {LabelCap} Stop ({silent}, {reason.Accepted}): {reason.Reason}");
        if (reason != default && !reason.Accepted && !silent)
            Messages.Message(reason.Reason, this, MessageTypeDefOf.RejectInput);
        ResetState(reason);
        return reason;
    }
}
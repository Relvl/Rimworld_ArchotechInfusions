using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.comps;

/// <summary>
/// Игрок может указать запустить процесс.
/// 1. Стартует зарядка конденсатора. Расходуется 10000 энергии.
/// 2. Стартует передача, высокое потребление, расходуется 1 ключ. 
/// 3. Стартует приём. Низкое потребление.
/// При потере питания прогресс теряется.
/// При отключении от грида, переполнении дешифраторов - новый процесс не может начаться.
///
/// Каждый рар тик, если State != Idle проверяется состояние батарей, дешифраторов, и авторежим.
/// </summary>
public class CompTransceiver : ProgressCompBase
{
    private bool _autostart;
    private TransceiverState _state;

    public CompPropsTransceiver Props => props as CompPropsTransceiver;

    public TransceiverState State
    {
        get => _state;
        set => _state = value;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _state, "state");
        Scribe_Values.Look(ref _autostart, "autostart");
    }

    public override void ReceiveCompSignal(string signal)
    {
        switch (signal)
        {
            case "FlickedOff":
            case "PowerTurnedOff":
                Stop("Transceiver losts power!");
                break;
        }
    }

    public void TryToStart()
    {
        var declineMessage = TryToStartInner();
        if (!declineMessage)
        {
            Stop(declineMessage.Reason);
        }
    }

    private AcceptanceReport TryToStartInner()
    {
        if (Power is null || Member is null) return "Inconsistent game defs";
        if (State != TransceiverState.Idle) return "Already in progress";

        var accumulators = Member.Grid.GetComps<CompAccumulator>();
        if (accumulators.Count == 0) return "Grid has no any accumulator";

        var totalStored = accumulators.Sum(a => a.Stored);
        var totalNeeded = Props.TranscieveConsumption + Props.ReceiveConsumption;
        if (totalStored < totalNeeded) return $"Not enought energy in the accumulators ({totalStored:0} / {totalNeeded:0})";

        StartStage(TransceiverState.Recharging);

        return true;
    }

    private void Stop(string message = default)
    {
        Progress = 0;
        State = TransceiverState.Idle;
        if (message != default)
        {
            Messages.Message(message, parent, MessageTypeDefOf.RejectInput);
        }
    }

    private void StartStage(TransceiverState newState)
    {
        State = newState;
        Progress = 0;

        if (!Mute)
            switch (newState)
            {
                case TransceiverState.Recharging:
                    ArchInfSoundDefOf.ArchInfTransceiverRecharge.PlayOneShot(parent);
                    break;
                case TransceiverState.Transceive:
                    ArchInfSoundDefOf.ArchInfTransceiverStart.PlayOneShot(parent);
                    break;
                case TransceiverState.Receive:
                    ArchInfSoundDefOf.ArchInfTransceiverReceive.PlayOneShot(parent);
                    break;
            }
    }

    private void Final()
    {
        Stop();
        Messages.Message("DEV: receive done!", parent, MessageTypeDefOf.PositiveEvent);
    }

    public override void CompTick()
    {
        switch (State)
        {
            case TransceiverState.Recharging:
                if (Progress >= Props.RechargeTicks)
                {
                    StartStage(TransceiverState.Transceive);
                    return;
                }

                var accumulators = Member.Grid.GetComps<CompAccumulator>();
                var totalNeeded = (Props.TranscieveConsumption + Props.ReceiveConsumption) / Props.RechargeTicks;
                foreach (var accumulator in accumulators)
                {
                    accumulator.Consume(ref totalNeeded);
                    if (totalNeeded <= 0) break;
                }

                if (totalNeeded > 0)
                {
                    Stop("Not enought power to recharge the Transmitter!");
                    return;
                }

                Progress++;
                break;

            case TransceiverState.Transceive:
                if (Progress >= Props.TransceiveTicks)
                {
                    StartStage(TransceiverState.Receive);
                    return;
                }

                Progress++;
                break;

            case TransceiverState.Receive:
                if (Progress >= Props.ReceiveTicks)
                {
                    Final();
                    return;
                }

                Progress++;
                break;
        }

        if (_autostart && parent.IsHashIntervalTick(250)) TryToStartInner();
    }

    public float GetStateProgressPercent()
    {
        switch (State)
        {
            case TransceiverState.Recharging:
                return Progress / Props.RechargeTicks;
            case TransceiverState.Transceive:
                return Progress / Props.TransceiveTicks;
            case TransceiverState.Receive:
                return Progress / Props.ReceiveTicks;
        }

        return 0;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (_state == TransceiverState.Idle)
        {
            yield return new Command_Action { defaultLabel = "Start", action = () => TryToStart() };
        }
        else
        {
            // todo! async dialog 
            yield return new Command_Action { defaultLabel = "STOP", action = () => Stop() };
        }

        yield return new Command_Action
        {
            defaultLabel = _autostart ? "Stop autostart" : "Autostart", //
            action = () => _autostart = !_autostart,
        };
        
        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        if (State != TransceiverState.Idle)
        {
            sb.AppendLine($"State: {State}");
            sb.Append($"Progress: {GetStateProgressPercent().ToStringPercent()}");
        }

        return sb.ToString();
    }
}

public enum TransceiverState
{
    Idle,
    Recharging,
    Transceive,
    Receive
}
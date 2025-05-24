using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class ArchInf_Transceiver_Building : AGridBuildingStateful
{
    private const int StateTransmit = 1;
    private const int StateReceive = 2;
    private const int StateLocked = 3;

    private bool _autostart;
    private bool _mute;
    private Comp_Transceiver _transceiver;
    private Comp_Transceiver Comp => _transceiver ??= GetComp<Comp_Transceiver>();

    private float TotalEnergy => Comp.Props.TransmitTicks * Comp.Props.TransmitPowerGain + Comp.Props.ReceiveTicks * Comp.Props.ReceivePowerGain;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _autostart, "autostart");
        Scribe_Values.Look(ref _mute, "mute");
    }

    public override void Tick()
    {
        base.Tick();
        if (State == StateIdle && this.IsHashIntervalTick(250) && _autostart && Grid.GetTotalEnergy() >= TotalEnergy * 1.1f)
            TryRun(!DebugSettings.ShowDevGizmos);
    }

    protected override void OnStateTick()
    {
        switch (State)
        {
            case StateTransmit:
                var energy = Comp.Props.TransmitPowerGain;
                Grid.ConsumeEnergy(ref energy);
                if (energy > 0)
                    Stop("JAI.Transceiver.Stop.NoEnergyToTransmit".Translate(), true);

                break;
            case StateReceive:
                energy = Comp.Props.ReceivePowerGain;
                Grid.ConsumeEnergy(ref energy);
                if (energy > 0)
                    Stop("JAI.Transceiver.Stop.NoEnergyToReceive".Translate(), true);
                break;
        }
    }

    protected override void OnStateComplete()
    {
        if (DebugSettings.ShowDevGizmos)
            Log.Message($"ArchInf_Transceiver_Building.OnStateComplete({State})");
        switch (State)
        {
            case StateTransmit:
                SetState(StateReceive, Comp.Props.ReceiveTicks);
                if (!_mute) ArchInfSoundDefOf.ArchInfTransceiverReceive.PlayOneShot(this);
                break;
            case StateReceive:
                var anyDecoderPresent = false;
                foreach (var decoder in Grid.Get<ArchInf_Decoder_Building>())
                {
                    anyDecoderPresent = true;
                    if (decoder.TryRun(!DebugSettings.ShowDevGizmos))
                    {
                        ResetState();
                        return;
                    }
                }

                SetState(StateLocked, 50);
                StateError = anyDecoderPresent ? "JAI.Error.WaitingForDecoder".Translate() : "JAI.Error.GridHasNoDecoder".Translate();
                break;
            case StateLocked:
                OnStateComplete();
                break;
        }
    }

    protected override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);
        switch (signal)
        {
            case "FlickedOff":
            case "PowerTurnedOff":
            case "Breakdown":
                if (State != StateIdle)
                    Stop("JAI.Error.FlickedOff".Translate(), true);
                break;
            case "FlickedOn":
            case "PowerTurnedOn":
                ResetState();
                break;
        }
    }

    protected override void GetStateDescription(StringBuilder sb)
    {
        base.GetStateDescription(sb);
        switch (State)
        {
            case StateTransmit:
                sb.AppendLine("JAI.State.Transmitting".Translate());
                break;
            case StateReceive:
                sb.AppendLine("JAI.State.Receiving".Translate());
                break;
            case StateLocked:
                sb.AppendLine("JAI.State.Locked".Translate());
                break;
        }
    }

    protected override void GetStateInspectString(StringBuilder sb)
    {
        switch (State)
        {
            case StateIdle:
                sb.AppendLine("JAI.Transceiver.PowerConsumption".Translate(TotalEnergy, Grid.GetTotalEnergy()));
                break;
            case StateTransmit:
                sb.AppendLine("JAI.Transceiver.PowerGain".Translate(Comp.Props.TransmitPowerGain));
                sb.AppendLine("JAI.Progress".Translate((GetPercentComplete() * 100f).ToString("0.")));
                break;
            case StateReceive:
                sb.AppendLine("JAI.Transceiver.PowerGain".Translate(Comp.Props.ReceivePowerGain));
                sb.AppendLine("JAI.Progress".Translate((GetPercentComplete() * 100f).ToString("0.")));
                break;
            case StateLocked:
                sb.AppendLine("JAI.Transceiver.Locked".Translate(Ticks));
                break;
        }
    }

    private float GetPercentComplete()
    {
        switch (State)
        {
            case StateIdle:
                return 0f;
            case StateTransmit:
                return 1f - (float)Ticks / Comp.Props.TransmitTicks;
            case StateReceive:
                return 1f - (float)Ticks / Comp.Props.ReceiveTicks;
            case StateLocked:
                return 1f;
            default:
                return 0f;
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        if (State == StateIdle)
            yield return new Command_Action
            {
                defaultLabel = "JAI.Gizmo.Transceiver.Start".Translate(),
                defaultDesc = "JAI.Gizmo.Transceiver.Start.Desc".Translate(),
                action = () =>
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    TryRun();
                }
            };
        else
            yield return new Command_Action
            {
                defaultLabel = "JAI.Gizmo.Transceiver.Stop".Translate(),
                defaultDesc = "JAI.Gizmo.Transceiver.Stop.Desc".Translate(),
                action = () =>
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Find.WindowStack.Add(
                        Dialog_MessageBox.CreateConfirmation(
                            "JAI.Gizmo.Transceiver.Stop.Confirm".Translate(),
                            () => Stop(default, true),
                            true,
                            "JAI.Gizmo.Transceiver.Stop".Translate()
                        )
                    );
                }
            };

        yield return new Command_Toggle
        {
            defaultLabel = _autostart ? "JAI.Gizmo.Transceiver.Autostart.ON".Translate() : "JAI.Gizmo.Transceiver.Autostart.OFF".Translate(), // todo render checkbox
            defaultDesc = "JAI.Gizmo.Transceiver.Autostart.Desc".Translate(),
            toggleAction = () =>
            {
                _autostart = !_autostart;
                if (!_autostart && State == StateIdle)
                    StateError = default;
            },
            isActive = () => _autostart
        };

        yield return new Command_Toggle
        {
            defaultLabel = _mute ? "JAI.Gizmo.Mute.ON".Translate() : "Mute: OFF".Translate(), // todo render checkbox
            defaultDesc = "JAI.Gizmo.Mute.Desc".Translate(LabelCap),
            toggleAction = () => _mute = !_mute,
            isActive = () => _mute
        };
    }

    protected virtual AcceptanceReport TryRun(bool silent = false)
    {
        if (State != StateIdle)
            return Message("JAI.Error.IsBusy".Translate(LabelCap), silent);

        if (Grid.Get<ArchInf_Accumulator_Building>().Empty())
            return Stop("JAI.Error.GridHasNoAccumulator".Translate(), silent);

        var totalStored = Grid.GetTotalEnergy();
        if (totalStored < TotalEnergy)
            return Stop("JAI.Error.BatteriesUncharged".Translate(totalStored.ToString("0"), TotalEnergy.ToString("0")), silent);

        var keyGenerators = Grid.Get<ArchInf_KeyGenerator_Building>();
        if (keyGenerators.Count == 0)
            return Stop("JAI.Error.GridHasNoKeyGenerator".Translate(), silent);

        if (!Enumerable.Any(keyGenerators, generator => generator.TryConsumeKey()))
            return Stop("JAI.Error.GridHasNoGeneratedKeys".Translate(), silent);

        SetState(StateTransmit, Comp.Props.TransmitTicks);
        if (!_mute) ArchInfSoundDefOf.ArchInfTransceiverStart.PlayOneShot(this);
        return true;
    }
}
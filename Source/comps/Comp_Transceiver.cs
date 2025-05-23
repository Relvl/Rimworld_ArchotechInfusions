using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchotechInfusions.comps.comp_base;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.comps;

public class CompProps_Transceiver : CompPropertiesBase_Grid
{
    public float ReceivePowerGain;
    public int ReceiveTicks;
    public float TransmitPowerGain;
    public int TransmitTicks;

    public CompProps_Transceiver()
    {
        compClass = typeof(Comp_Transceiver);
    }
}

public class Comp_Transceiver : CompBase_GridState<Comp_Transceiver, CompProps_Transceiver>
{
    private bool _autostart;
    private bool _mute;

    private float TotalEnergy => Props.TransmitTicks * Props.TransmitPowerGain + Props.ReceiveTicks * Props.ReceivePowerGain;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _autostart, "autostart");
        Scribe_Values.Look(ref _mute, "mute");
    }

    public override void ReceiveCompSignal(string signal)
    {
        switch (signal)
        {
            case "FlickedOff":
            case "PowerTurnedOff":
            case "Breakdown":
                if (CurrentState != Idle)
                {
                    Stop("JAI.Error.FlickedOff".Translate(), true);
                    CurrentState = Idle;
                }

                break;
            case "FlickedOn":
            case "PowerTurnedOn":
                CurrentState = Idle;
                Error = default;
                break;
        }
    }

    public override void CompTick()
    {
        if (CurrentState == Idle && _autostart && Parent.IsHashIntervalTick(100) && Grid.GetTotalEnergy() >= TotalEnergy * 1.1f)
            TryRun(true);
        base.CompTick();
    }

    public override AcceptanceReport TryRun(bool silent = false)
    {
        if (CurrentState != Idle)
            return Message("JAI.Error.IsBusy".Translate(Parent.LabelCap), silent);

        if (!Power.PowerOn)
            return Stop("JAI.Error.IsPoweredOff".Translate(Parent.LabelCap), silent);

        if (Grid.Get<Comp_Accumulator>().Empty())
            return Stop("JAI.Error.GridHasNoAccumulator".Translate(), silent);

        var totalStored = Grid.GetTotalEnergy();
        if (totalStored < TotalEnergy)
            return Stop("JAI.Error.BatteriesUncharged".Translate(totalStored.ToString("0"), TotalEnergy.ToString("0")), silent);

        var keyGenerators = Grid.Get<Comp_KeyGenerator>();
        if (keyGenerators.Count == 0)
            return Stop("JAI.Error.GridHasNoKeyGenerator".Translate(), silent);

        if (!Enumerable.Any(keyGenerators, generator => generator.TryConsumeKey()))
            return Stop("JAI.Error.GridHasNoGeneratedKeys".Translate(), silent);

        CurrentState = new StageTransmit(Props.TransmitTicks);
        return true;
    }

    public override bool OnComplete()
    {
        var anyDecoderPresent = false;
        foreach (var decoder in Grid.Get<Comp_Decoder>())
        {
            anyDecoderPresent = true;
            var started = decoder.TryRun(true);
            if (started)
            {
                Stop(default, true);
                return true;
            }
        }

        Error = anyDecoderPresent ? "JAI.Error.WaitingForDecoder".Translate() : "JAI.Error.GridHasNoDecoder".Translate();
        return false;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (CurrentState == Idle)
            yield return new Command_Action
            {
                defaultLabel = "JAI.Gizmo.Transceiver.Start".Translate(),
                defaultDesc = "JAI.Gizmo.Transceiver.Start.Desc".Translate(),
                action = () => TryRun()
            };
        else
            yield return new Command_Action
            {
                defaultLabel = "JAI.Gizmo.Transceiver.Stop".Translate(),
                defaultDesc = "JAI.Gizmo.Transceiver.Stop.Desc".Translate(),
                action = () =>
                {
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

        yield return new Command_Action
        {
            defaultLabel = _autostart ? "JAI.Gizmo.Transceiver.Autostart.ON".Translate() : "JAI.Gizmo.Transceiver.Autostart.OFF".Translate(), // todo render checkbox
            defaultDesc = "JAI.Gizmo.Transceiver.Autostart.Desc".Translate(),
            action = () =>
            {
                _autostart = !_autostart;
                if (!_autostart && CurrentState == Idle)
                    Error = default;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = _mute ? "JAI.Gizmo.Mute.ON".Translate() : "Mute: OFF".Translate(), // todo render checkbox
            defaultDesc = "JAI.Gizmo.Mute.Desc".Translate(Parent.LabelCap),
            action = () => _mute = !_mute
        };

        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;
    }

    protected override void FillInstectStringExtra(StringBuilder sb)
    {
        sb.AppendLine("JAI.Transceiver.PowerConsumption".Translate(TotalEnergy));
    }

    private class StageTransmit(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Transmitting".Translate();

        public override void OnFirstTick(Comp_Transceiver owner)
        {
            if (!owner._mute)
                ArchInfSoundDefOf.ArchInfTransceiverStart.PlayOneShot(owner.Parent);
        }

        public override void OnCompTick(Comp_Transceiver owner)
        {
            var energy = owner.Props.TransmitPowerGain;
            owner.Grid.ConsumeEnergy(ref energy);
            if (energy > 0)
                owner.Stop("JAI.Transceiver.Stop.NoEnergyToTransmit".Translate(), false);
        }


        public override void OnProgressComplete(Comp_Transceiver owner)
        {
            owner.CurrentState = new StageReceive(owner.Props.ReceiveTicks);
        }

        public override void CompInspectStringExtra(StringBuilder sb, Comp_Transceiver owner)
        {
            sb.AppendLine("JAI.Transceiver.PowerGain".Translate(owner.Props.TransmitPowerGain));
            base.CompInspectStringExtra(sb, owner);
        }
    }

    private class StageReceive(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Receiving".Translate();

        public override void OnFirstTick(Comp_Transceiver owner)
        {
            if (!owner._mute)
                ArchInfSoundDefOf.ArchInfTransceiverReceive.PlayOneShot(owner.Parent);
        }

        public override void OnCompTick(Comp_Transceiver owner)
        {
            var energy = owner.Props.ReceivePowerGain;
            owner.Grid.ConsumeEnergy(ref energy);
            if (energy > 0)
                owner.Stop("JAI.Transceiver.Stop.NoEnergyToReceive".Translate(), false);
        }

        public override void OnProgressComplete(Comp_Transceiver owner)
        {
            owner.OnComplete();
        }

        public override void CompInspectStringExtra(StringBuilder sb, Comp_Transceiver owner)
        {
            sb.AppendLine("JAI.Transceiver.PowerGain".Translate(owner.Props.ReceivePowerGain));
            base.CompInspectStringExtra(sb, owner);
        }
    }
}
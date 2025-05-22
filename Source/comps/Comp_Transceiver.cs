using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps.comp_base;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global, InconsistentNaming, ClassNeverInstantiated.Global -- comp reflective
public class CompProps_Transceiver : CompProperties
{
    public int ReceiveConsumption;

    public int ReceiveTicks;
    public int RechargeTicks;
    public int TransceiveTicks;
    public int TranscieveConsumption;

    public CompProps_Transceiver()
    {
        compClass = typeof(Comp_Transceiver);
    }
}

public class Comp_Transceiver : CompBase_GridState<Comp_Transceiver, CompProps_Transceiver>
{
    private bool _autostart;
    private bool _mute;

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
        if (CurrentState == Idle && _autostart && parent.IsHashIntervalTick(100))
            TryRun(true);
        base.CompTick();
    }

    public override AcceptanceReport TryRun(bool silent = false)
    {
        if (CurrentState != Idle)
            return Message("JAI.Error.IsBusy".Translate(parent.LabelCap), silent);

        if (!Power.PowerOn)
            return Stop("JAI.Error.IsPoweredOff".Translate(parent.LabelCap), silent);

        if (Member.Grid.GetComps<Comp_Accumulator>().Empty())
            return Stop("JAI.Error.GridHasNoAccumulator".Translate(), silent);

        var totalStored = Member.Grid.GetTotalEnergy();
        var totalNeeded = Props.TranscieveConsumption + Props.ReceiveConsumption;
        if (totalStored < totalNeeded)
            return Stop("JAI.Error.BatteriesUncharged".Translate(totalStored.ToString("0"), totalNeeded.ToString("0")), silent);

        var keyGenerators = Member.Grid.GetComps<Comp_KeyGenerator>();
        if (keyGenerators.Count == 0)
            return Stop("JAI.Error.GridHasNoKeyGenerator".Translate(), silent);

        if (!Enumerable.Any(keyGenerators, generator => generator.TryConsumeKey()))
            return Stop("JAI.Error.GridHasNoGeneratedKeys".Translate(), silent);

        CurrentState = new StageRecharge(Props.RechargeTicks);
        return true;
    }

    public override bool OnComplete()
    {
        var anyDecoderPresent = false;
        foreach (var decoder in Member.Grid.GetComps<Comp_Decoder>())
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
            defaultDesc = "JAI.Gizmo.Mute.Desc".Translate(parent.LabelCap),
            action = () => _mute = !_mute
        };

        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;
    }

    private class StageRecharge(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Recharging".Translate();

        public override void OnFirstTick(Comp_Transceiver owner)
        {
            if (owner._mute) return;
            ArchInfSoundDefOf.ArchInfTransceiverRecharge.PlayOneShot(owner.parent);
        }

        public override void OnCompTick(Comp_Transceiver owner)
        {
            var totalNeeded = (float)(owner.Props.TranscieveConsumption + owner.Props.ReceiveConsumption) / owner.Props.RechargeTicks;
            owner.Member.Grid.ConsumeEnergy(ref totalNeeded);

            if (totalNeeded > 0)
                owner.Stop("JAI.Error.Transmitter.NoPowerForRecharge".Translate(), true);
        }

        public override void OnProgressComplete(Comp_Transceiver owner)
        {
            owner.CurrentState = new StageTransmit(owner.Props.TransceiveTicks);
        }
    }

    private class StageTransmit(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Transmitting".Translate();

        public override void OnFirstTick(Comp_Transceiver owner)
        {
            if (owner._mute) return;
            ArchInfSoundDefOf.ArchInfTransceiverStart.PlayOneShot(owner.parent);
        }

        public override void OnProgressComplete(Comp_Transceiver owner)
        {
            owner.CurrentState = new StageReceive(owner.Props.ReceiveTicks);
        }
    }

    private class StageReceive(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Receiving".Translate();

        public override void OnFirstTick(Comp_Transceiver owner)
        {
            if (owner._mute) return;
            ArchInfSoundDefOf.ArchInfTransceiverReceive.PlayOneShot(owner.parent);
        }

        public override void OnProgressComplete(Comp_Transceiver owner)
        {
            owner.OnComplete();
        }
    }
}
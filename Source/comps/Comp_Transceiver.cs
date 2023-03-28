using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global, InconsistentNaming, ClassNeverInstantiated.Global -- comp reflective
public class CompProps_Transceiver : CompProperties
{
    public int RechargeTicks;

    public int ReceiveConsumption;
    public int TranscieveConsumption;

    public int ReceiveTicks;
    public int TransceiveTicks;

    public CompProps_Transceiver() => compClass = typeof(Comp_Transceiver);
}

public class Comp_Transceiver : CompBase_Stageable<Comp_Transceiver>
{
    private bool _autostart;

    private CompProps_Transceiver Props => props as CompProps_Transceiver;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _autostart, "autostart");
    }

    public override void ReceiveCompSignal(string signal)
    {
        switch (signal)
        {
            case "FlickedOff":
            case "PowerTurnedOff":
                StopAction("Transceiver losts power while in progress!");
                break;
        }
    }

    public override void CompTick()
    {
        if (CurrentState == Idle)
        {
            if (_autostart && parent.IsHashIntervalTick(200)) StartAction(true);
            return;
        }

        base.CompTick();
    }

    public override AcceptanceReport CanStart(bool silent = false)
    {
        if (CurrentState != Idle) return Message($"{parent.LabelCap} is busy", silent);
        if (!Power.PowerOn) return Message($"{parent.LabelCap} is out of power", silent);

        // todo check key generated

        var accumulators = Member.Grid.GetComps<CompAccumulator>();
        if (accumulators.Count == 0) return Message("Grid has no accumulator", silent);

        var totalStored = accumulators.Sum(a => a.Stored);
        var totalNeeded = Props.TranscieveConsumption + Props.ReceiveConsumption;
        if (totalStored < totalNeeded) return Message($"Not enought energy in the accumulators ({totalStored:0} / {totalNeeded:0})", silent);

        return true;
    }

    public override AcceptanceReport StartAction(bool silent = false)
    {
        var acceptanceReport = CanStart(silent);
        if (!acceptanceReport) return acceptanceReport;

        // todo reserve the key
        CurrentState = new StageRecharge(Props.RechargeTicks);

        return true;
    }

    public override bool FinalAction()
    {
        var decoder = Member.Grid.GetComps<Comp_Decoder>().FirstOrDefault(d => d.CanStart(true));
        if (decoder != default)
        {
            if (DebugSettings.ShowDevGizmos) Messages.Message("DEV: receive done - go decode!", parent, MessageTypeDefOf.PositiveEvent);
            decoder.StartAction(true);
            CurrentState = Idle;
            return true;
        }

        return false;
    }

    protected override void StopAction(string reason = default)
    {
        // todo! release reserved key
        base.StopAction(reason);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (CurrentState == Idle)
            yield return new Command_Action { defaultLabel = "Start", action = () => StartAction() };
        else
            // todo! async dialog 
            yield return new Command_Action
            {
                defaultLabel = "STOP",
                action = () =>
                {
                    Find.WindowStack.Add(
                        Dialog_MessageBox.CreateConfirmation( //
                            "Really want to stop process? All data and energy will be lost!",
                            () => StopAction(),
                            true,
                            "Stop"
                        )
                    );
                }
            };

        yield return new Command_Action
        {
            defaultLabel = _autostart ? "Stop autostart" : "Autostart", //
            action = () => _autostart = !_autostart,
        };

        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
    }

    private class StageRecharge : State
    {
        public override string Label => "Recharging".Translate();

        public StageRecharge(int ticks) : base(ticks)
        {
        }

        public override void OnFirstTick(Comp_Transceiver owner) => ArchInfSoundDefOf.ArchInfTransceiverRecharge.PlayOneShot(owner.parent);

        public override void OnCompTick(Comp_Transceiver owner)
        {
            var accumulators = owner.Member.Grid.GetComps<CompAccumulator>();
            var totalNeeded = (float)(owner.Props.TranscieveConsumption + owner.Props.ReceiveConsumption) / owner.Props.RechargeTicks;
            foreach (var accumulator in accumulators)
            {
                accumulator.Consume(ref totalNeeded);
                if (totalNeeded <= 0) break;
            }

            if (totalNeeded > 0)
            {
                owner.StopAction("Not enought power to recharge the Transmitter!");
            }
        }

        public override void OnProgressComplete(Comp_Transceiver owner) => owner.CurrentState = new StageTransmit(owner.Props.TransceiveTicks);
    }

    private class StageTransmit : State
    {
        public override string Label => "Transmitting".Translate();

        public StageTransmit(int ticks) : base(ticks)
        {
        }

        public override void OnFirstTick(Comp_Transceiver owner) => ArchInfSoundDefOf.ArchInfTransceiverStart.PlayOneShot(owner.parent);
        public override void OnProgressComplete(Comp_Transceiver owner) => owner.CurrentState = new StageReceive(owner.Props.ReceiveTicks);
    }

    private class StageReceive : State
    {
        public override string Label => "Receiving".Translate();

        public StageReceive(int ticks) : base(ticks)
        {
        }

        public override void OnFirstTick(Comp_Transceiver owner) => ArchInfSoundDefOf.ArchInfTransceiverReceive.PlayOneShot(owner.parent);
        public override void OnProgressComplete(Comp_Transceiver owner) => owner.FinalAction();
    }
}
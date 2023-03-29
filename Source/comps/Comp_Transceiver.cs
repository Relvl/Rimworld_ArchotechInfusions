using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

public class Comp_Transceiver : CompBase_Stageable<Comp_Transceiver, CompProps_Transceiver>
{
    private bool _autostart;
    private Guid _key;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _autostart, "autostart");
        Scribe_Values.Look(ref _key, "key");
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

        var keyGenerators = Member.Grid.GetComps<Comp_KeyGenerator>();
        if (keyGenerators.Count == 0) return Message("Grid has no key generator", silent);

        var accumulators = Member.Grid.GetComps<Comp_Accumulator>();
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

        var keyGenerators = Member.Grid.GetComps<Comp_KeyGenerator>();
        var key = keyGenerators.FirstOrDefault(g => g.HasFreeKey())?.ConsumeKey();
        if (key == default) return Message("Grid has no generated keys", silent);

        CurrentState = new StageRecharge(Props.RechargeTicks);
        // todo fail statuses
        LastCheckStatus = default;

        return true;
    }

    public override bool FinalAction()
    {
        var decoder = Member.Grid.GetComps<Comp_Decoder>().FirstOrDefault(d => d.CanStart(true));
        if (decoder != default)
        {
            if (DebugSettings.ShowDevGizmos) Messages.Message("DEV: receive done - go decode!", parent, MessageTypeDefOf.PositiveEvent);
            if (decoder.StartAction(true)) decoder.SetupKey(_key);
            _key = default;
            CurrentState = Idle;
            return true;
        }

        return false;
    }

    protected override void StopAction(string reason = default)
    {
        _key = default;
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
                            "Really want to stop process? The key, all data and energy will be lost!",
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
            var accumulators = owner.Member.Grid.GetComps<Comp_Accumulator>();
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
        private bool _showNeedDecoder;
        public override string Label => "Receiving".Translate();

        public StageReceive(int ticks) : base(ticks)
        {
        }

        public override void OnFirstTick(Comp_Transceiver owner) => ArchInfSoundDefOf.ArchInfTransceiverReceive.PlayOneShot(owner.parent);

        public override void OnProgressComplete(Comp_Transceiver owner) => _showNeedDecoder = !owner.FinalAction();

        public override bool CompInspectStringExtra(StringBuilder sb, Comp_Transceiver owner)
        {
            base.CompInspectStringExtra(sb, owner);
            if (_showNeedDecoder)
            {
                sb.AppendLine("Waiting for free decoder...");
                return false;
            }

            return true;
        }
    }
}
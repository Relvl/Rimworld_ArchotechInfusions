using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.statcollectors;
using ArchotechInfusions.ui;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global, InconsistentNaming, ClassNeverInstantiated.Global -- comp reflective
public class CompProps_Decoder : CompProperties
{
    public int StartupTicks;
    public IntRange DecodeTicks;

    public CompProps_Decoder() => compClass = typeof(Comp_Decoder);
}

public class Comp_Decoder : CompBase_Stageable<Comp_Decoder, CompProps_Decoder>
{
    private State _backupState;
    private int _backupProgress;
    private State _startup;
    private Guid _key;
    private Instruction _modifier;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _key, "key");
        Scribe_Deep.Look(ref _modifier, "storedModifier");
        if (ScribeState(ref _backupState, "backupState"))
        {
            Scribe_Values.Look(ref _backupProgress, "backupProgress");
        }
    }

    public override void CompTick()
    {
        if (!Power.PowerOn)
        {
            if (CurrentState != Idle && _backupState == default)
            {
                _backupState = CurrentState;
                _backupProgress = progress;
            }

            CurrentState = _startup ??= new StateStartup(Props.StartupTicks);
        }

        if (_modifier is not null && Power.PowerOn)
        {
            var databases = Member.Grid.GetComps<Comp_Database>();
            foreach (var compDatabase in databases)
                if (compDatabase.MakeDatabaseRecord(_modifier))
                {
                    _modifier = null;
                    break;
                }
        }

        base.CompTick();
    }

    public override AcceptanceReport CanStart(bool silent = false)
    {
        if (CurrentState != Idle) return Message($"{parent.LabelCap} is busy", silent);
        if (!Power.PowerOn) return Message($"{parent.LabelCap} is out of power", silent);
        if (_modifier is not null) return Message($"{parent.LabelCap} has stored data packet", silent);
        return true;
    }

    public override AcceptanceReport StartAction(bool silent = false)
    {
        var acceptanceReport = CanStart(silent);
        if (!acceptanceReport) return acceptanceReport;

        CurrentState = new StateDecode(Props.DecodeTicks.RandomInRange);

        return true;
    }

    public void SetupKey(Guid key)
    {
        _key = key;
    }

    public override bool FinalAction()
    {
        CurrentState = Idle;
        _modifier = StatCollector.GenerateNewInstruction();
        _key = default;
        if (DebugSettings.ShowDevGizmos) Messages.Message("DEV: decode done!", parent, MessageTypeDefOf.PositiveEvent);
        return true;
    }

    protected override void StopAction(string reason = default)
    {
        base.StopAction(reason);
        _key = default;
        _backupProgress = default;
        _backupState = default;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Try decode", action = () => StartAction() };
            yield return new Command_Action { defaultLabel = "Call final", action = () => FinalAction() };
            yield return new Command_Action
            {
                defaultLabel = "Show available stats",
                action = () =>
                {
                    Find.WindowStack.TryRemove(typeof(ShowElementsWindow));
                    Find.WindowStack.Add(new ShowElementsWindow());
                }
            };
        }

        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
    }

    private class StateStartup : State
    {
        public override string Label => "Startup".Translate();

        public StateStartup(int ticks) : base(ticks)
        {
        }

        public override void OnFirstTick(Comp_Decoder owner)
        {
            // todo! sound
        }

        public override void OnProgressComplete(Comp_Decoder owner)
        {
            if (owner._backupState != default)
            {
                owner.CurrentState = owner._backupState;
                owner.progress = owner._backupProgress;
                owner._backupState = default;
                owner._backupProgress = default;
                return;
            }

            owner.CurrentState = new StateDecode(owner.Props.DecodeTicks.RandomInRange);
        }

        public override bool CompInspectStringExtra(StringBuilder sb, Comp_Decoder owner)
        {
            base.CompInspectStringExtra(sb, owner);
            if (owner._backupState != default)
            {
                sb.AppendLine($"Stored decode progress: {owner._backupProgress / (float)owner._backupState.Ticks * 100f:0.}%");
            }

            return true;
        }
    }

    private class StateDecode : State
    {
        public override string Label => "Decoding".Translate();

        public StateDecode(int ticks) : base(ticks)
        {
        }

        public override void OnFirstTick(Comp_Decoder owner)
        {
            // todo sound
        }

        public override void OnProgressComplete(Comp_Decoder owner) => owner.FinalAction();
    }
}
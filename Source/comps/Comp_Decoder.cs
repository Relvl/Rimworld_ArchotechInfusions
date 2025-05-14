using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.statcollectors;
using ArchotechInfusions.ui;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global, InconsistentNaming, ClassNeverInstantiated.Global -- comp reflective
public class CompProps_Decoder : CompProperties
{
    public IntRange DecodeTicks;
    public int StartupTicks;

    public CompProps_Decoder()
    {
        compClass = typeof(Comp_Decoder);
    }
}

public class Comp_Decoder : CompBase_GridState<Comp_Decoder, CompProps_Decoder>
{
    private int _backupProgress;
    private State _backupState;
    private Instruction _instruction;
    private bool _mute;
    private State _startup;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref _instruction, "storedInstruction");
        Scribe_Values.Look(ref _mute, "mute");
        if (ScribeState(ref _backupState, "backupState")) Scribe_Values.Look(ref _backupProgress, "backupProgress");
    }

    public override void CompTick()
    {
        if (!Power.PowerOn)
        {
            if (CurrentState != Idle && CurrentState is not StateStartup && _backupState == default)
            {
                _backupState = CurrentState;
                _backupProgress = progress;
            }

            CurrentState = _startup ??= new StateStartup(Props.StartupTicks);
        }

        base.CompTick();
    }

    public override AcceptanceReport TryRun(bool silent = false)
    {
        if (CurrentState != Idle)
            return Message("JAI.Error.IsBusy".Translate(parent.LabelCap), silent);
        if (!Power.PowerOn)
            return Message("JAI.Error.IsPoweredOff".Translate(parent.LabelCap), silent);
        if (_instruction is not null)
            return Message("JAI.Error.Decoder.StoredInstruction".Translate(parent.LabelCap), silent);

        CurrentState = new StateDecode(Props.DecodeTicks.RandomInRange);
        return true;
    }

    public override bool OnComplete()
    {
        _instruction ??= StatCollector.GenerateNewInstruction();

        var anyDatabasePresent = false;
        foreach (var compDatabase in Member.Grid.GetComps<Comp_Database>())
        {
            anyDatabasePresent = true;
            if (compDatabase.MakeDatabaseRecord(_instruction))
            {
                Stop(default, true);
                return true;
            }
        }

        Error = anyDatabasePresent ? "JAI.Error.WaitingForDatabase".Translate() : "JAI.Error.GridHasNoDatabase".Translate();
        return false;
    }

    protected override AcceptanceReport Stop(string reason, bool silent)
    {
        _instruction = null;
        return base.Stop(reason, silent);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = _mute ? "JAI.Gizmo.Mute.ON".Translate() : "Mute: OFF".Translate(), // todo render checkbox
            defaultDesc = "JAI.Gizmo.Mute.Desc".Translate(parent.LabelCap),
            action = () => _mute = !_mute
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Try decode", action = () => TryRun() };
            yield return new Command_Action { defaultLabel = "Force decode", action = () => OnComplete() };
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

        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;
    }

    private class StateStartup(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Startup".Translate();

        public override void OnFirstTick(Comp_Decoder owner)
        {
            // todo! sound
        }

        public override void OnProgressComplete(Comp_Decoder owner)
        {
            owner.CurrentState = Idle;
            if (owner._backupState != default)
            {
                owner.CurrentState = owner._backupState;
                owner.progress = owner._backupProgress;
                owner._backupState = default;
                owner._backupProgress = default;
            }
        }

        public override void CompInspectStringExtra(StringBuilder sb, Comp_Decoder owner)
        {
            if (owner._backupState != default)
            {
                var progress = owner._backupProgress / (float)owner._backupState.Ticks * 100f;
                sb.AppendLine("JAI.Decoder.StoredBackup".Translate(progress.ToString("0.")));
            }

            base.CompInspectStringExtra(sb, owner);
        }
    }

    private class StateDecode(int ticks) : State(ticks)
    {
        public override string Label => "JAI.State.Decoding".Translate();

        public override void OnFirstTick(Comp_Decoder owner)
        {
            if (owner._mute) return;
            ArchInfSoundDefOf.ArchInfDecoderStart.PlayOneShot(owner.parent);
        }

        public override void OnProgressComplete(Comp_Decoder owner)
        {
            owner.OnComplete();
        }
    }
}
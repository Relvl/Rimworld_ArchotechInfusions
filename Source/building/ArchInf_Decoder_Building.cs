using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ArchInf_Decoder_Building : AGridBuildingStateful
{
    private const int StateStartupKey = 1;
    private const int StateDecodingKey = 2;

    private int _backupProgress;

    private Comp_Decoder _comp;
    private AInstruction _instruction;
    private bool _mute;
    public Comp_Decoder Comp => _comp ??= GetComp<Comp_Decoder>();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref _instruction, "storedInstruction");
        Scribe_Values.Look(ref _mute, "mute");
        Scribe_Values.Look(ref _backupProgress, "backupProgress");
    }

    public override void Tick()
    {
        base.Tick();
        if (!Grid.PowerOn)
        {
            if (State == StateDecodingKey && _backupProgress == default)
                _backupProgress = Ticks;

            SetState(StateStartupKey, Comp.Props.StartupTicks);
        }
        else
        {
            if (State == StateIdle)
            {
                var energy = Comp.Props.IdleEnergyGain;
                Grid.ConsumeEnergy(ref energy);
            }
        }
    }

    protected override void OnStateTick()
    {
        switch (State)
        {
            case StateStartupKey:
            {
                var energy = Comp.Props.StartupEnergyGain;
                Grid.ConsumeEnergy(ref energy);
                if (energy > 0)
                {
                    // todo flick
                }

                break;
            }
            case StateDecodingKey:
            {
                var energy = Comp.Props.DecodingEnergyGain;
                Grid.ConsumeEnergy(ref energy);
                if (energy > 0)
                {
                    // todo flick
                }

                break;
            }
        }
    }

    protected override void OnStateComplete()
    {
        switch (State)
        {
            case StateStartupKey:
                if (_backupProgress > 0)
                {
                    if (!_mute) ArchInfSoundDefOf.ArchInfDecoderStart.PlayOneShot(this);
                    SetState(StateDecodingKey, _backupProgress);
                    _backupProgress = default;
                }
                else
                {
                    SetState(StateIdle);
                }

                break;
            case StateDecodingKey:
                _instruction ??= StatProcessor.GenerateInstruction();
                if (Grid.TryPutInstruction(_instruction))
                {
                    StateError = default;
                    SetState(StateIdle);
                    _instruction = null;
                    _backupProgress = default;
                }
                else
                {
                    StateError = Grid.Get<ArchInf_Database_Building>().Any() ? "JAI.Error.WaitingForDatabase".Translate() : "JAI.Error.GridHasNoDatabase".Translate();
                    SetState(StateDecodingKey, 50);
                }

                break;
        }
    }

    public virtual AcceptanceReport TryRun(bool silent = false)
    {
        if (State != StateIdle)
            return Message("JAI.Error.IsBusy".Translate(Comp.Parent.LabelCap), silent);
        if (!Grid.PowerOn)
            return Message("JAI.Error.IsPoweredOff".Translate(Comp.Parent.LabelCap), silent);
        if (_instruction is not null)
            return Message("JAI.Error.Decoder.StoredInstruction".Translate(Comp.Parent.LabelCap), silent);

        if (TicksInitial == 0)
            TicksInitial = Comp.Props.DecodeTicks.RandomInRange;

        if (Grid.GetTotalEnergy() < Comp.Props.DecodingEnergyGain * TicksInitial * 1.1f)
            return Message("JAI.Error.BatteriesUncharged".Translate(Grid.GetTotalEnergy().ToString("0"), (Comp.Props.DecodingEnergyGain * TicksInitial * 1.1f).ToString("0")), silent);

        if (!_mute)
            ArchInfSoundDefOf.ArchInfDecoderStart.PlayOneShot(this);
        SetState(StateDecodingKey, TicksInitial);
        return true;
    }

    protected override void GetStateDescription(StringBuilder sb)
    {
        base.GetStateDescription(sb);
        switch (State)
        {
            case StateStartupKey:
                sb.AppendLine("JAI.State.Startup".Translate());
                break;
            case StateDecodingKey:
                break;
        }
    }

    protected override void GetStateInspectString(StringBuilder sb)
    {
        base.GetStateInspectString(sb);
        switch (State)
        {
            case StateStartupKey:
                if (_backupProgress != default)
                    sb.AppendLine("JAI.Decoder.StoredBackup".Translate((_backupProgress / (float)TicksInitial * 100f).ToString("0.")));
                break;
            case StateDecodingKey:
                sb.AppendLine("JAI.Decoder.Decoding".Translate((Ticks / (float)TicksInitial * 100f).ToString("0.")));
                break;
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        yield return new Command_Action
        {
            defaultLabel = _mute ? "JAI.Gizmo.Mute.ON".Translate() : "Mute: OFF".Translate(), // todo render checkbox
            defaultDesc = "JAI.Gizmo.Mute.Desc".Translate(LabelCap),
            action = () => _mute = !_mute
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Try decode", action = () => TryRun() };
            yield return new Command_Action { defaultLabel = "Force decode", action = () => OnStateComplete() };
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
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ArchotechInfusions.comps;

public class CompDecoder : ProgressCompBase
{
    private DecoderState _state;

    private string _decodedData;

    private int _backupProgress;
    private int _workTicks;

    public CompPropsDecoder Props => props as CompPropsDecoder;

    public DecoderState State
    {
        get => _state;
        set => _state = value;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        if (Scribe.mode == LoadSaveMode.Saving && State == DecoderState.Decoding)
        {
            _backupProgress = Progress;
            State = DecoderState.StartingUp;
        }

        Scribe_Values.Look(ref _state, "state");
        Scribe_Values.Look(ref _decodedData, "decoded");
        Scribe_Values.Look(ref _backupProgress, "backupProgress");
        Scribe_Values.Look(ref _workTicks, "workTicks");
    }

    public void Final()
    {
        _workTicks = 0;
        _backupProgress = 0;
        _decodedData = Guid.NewGuid().ToString(); // todo! generate infusion
        State = DecoderState.Idle;
        Progress = 0;
    }

    public override void CompTick()
    {
        if (!Power.PowerOn)
        {
            if (_backupProgress == default)
            {
                _backupProgress = Progress;
                State = DecoderState.StartingUp;
                Progress = 0;
            }

            return;
        }

        if (_decodedData != default)
        {
            
            return;
        }

        switch (State)
        {
            case DecoderState.StartingUp:
                if (Progress >= Props.StartupTicks)
                {
                    if (_backupProgress != default)
                    {
                        Progress = _backupProgress;
                        State = DecoderState.Decoding;
                        _backupProgress = 0;
                        return;
                    }

                    Progress = 0;
                    State = DecoderState.Idle;
                    return;
                }

                Progress++;
                break;
            case DecoderState.Decoding:
                if (Progress >= _workTicks)
                {
                    Final();
                    return;
                }

                Progress++;
                break;
        }

        base.CompTick();
    }

    public bool TryDecode()
    {
        if (State != DecoderState.Idle) return false;
        if (_decodedData != default) return false;
        if (_backupProgress != default) return false;
        if (!Power.PowerOn) return false;

        _workTicks = (int)Props.DecodeTicks.RandomInRange;
        Progress = 0;
        State = DecoderState.Decoding;
        return true;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Try decode", action = () => TryDecode() };
            yield return new Command_Action { defaultLabel = "Call final", action = Final };
        }

        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        if (State != DecoderState.Idle)
        {
            sb.AppendLine($"State: {State}");
            sb.Append($"Progress: {Progress}");
        }

        return sb.ToString();
    }
}

public enum DecoderState
{
    StartingUp,
    Idle,
    Decoding
}
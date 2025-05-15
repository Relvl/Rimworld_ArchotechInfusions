using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.comps.comp_base;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global, InconsistentNaming, ClassNeverInstantiated.Global -- comp reflective
public class CompProps_Accumulator : CompProperties
{
    public float MaxStored;

    public CompProps_Accumulator()
    {
        compClass = typeof(Comp_Accumulator);
    }
}

public class Comp_Accumulator : CompBase_Grid<CompProps_Accumulator>
{
    private float _stored;
    private float _storedLastTick;

    public float Stored
    {
        get => _stored;
        private set => _stored = Math.Max(0, Math.Min(value, Props.MaxStored));
    }

    private bool IsFull => Stored >= Props.MaxStored;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _stored, "stored");

        Stored = _stored;
        _storedLastTick = _stored;
    }

    public override void CompTick()
    {
        _storedLastTick = Stored;

        if (Power.PowerOn)
        {
            if (IsFull)
            {
                Power.PowerOutput = -Power.Props.idlePowerDraw;
            }
            else
            {
                Power.PowerOutput = -Power.Props.PowerConsumption;
                Stored -= Power.PowerOutput * CompPower.WattsToWattDaysPerTick * 10;
            }
        }

        base.CompTick();
    }

    public bool Consume(ref float energy)
    {
        // todo! balance tha accums
        if (Stored >= energy)
        {
            Stored -= energy;
            energy = 0;
            return true;
        }

        energy -= Math.Min(Stored, energy);
        Stored = 0;
        return false;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        if (DebugSettings.ShowDevGizmos) sb.AppendLine($"Diff: {(Stored - _storedLastTick) * CompPower.WattsToWattDaysPerTick:+0.##;0;-0.##}");
        sb.Append($"Stored: {Stored:0.} / {Props.MaxStored:0.}");
        return sb.ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "DEV: Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "DEV: -10%", action = () => Stored -= Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: +10%", action = () => Stored += Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: Full", action = () => Stored = Props.MaxStored };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class CompProps_Accumulator : CompPropertiesBase_Grid
{
    public float MaxStored;

    public CompProps_Accumulator()
    {
        compClass = typeof(Comp_Accumulator);
    }
}

[StaticConstructorOnStartup]
public class Comp_Accumulator : CompBase_Grid<CompProps_Accumulator>
{
    private static Texture2D dischargeTex;

    private bool _markedToDischarge;
    private float _stored;

    public float Stored
    {
        get => _stored;
        set => _stored = Math.Max(0, Math.Min(value, Props.MaxStored));
    }

    public bool MarkedToDischarge => _markedToDischarge;

    public bool IsFull => Stored >= Props.MaxStored;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _stored, nameof(Stored));
        Scribe_Values.Look(ref _markedToDischarge, "MarkedToDischarge");

        Stored = _stored;
    }

    public override void CompTick()
    {
        if (Power.PowerOn && !_markedToDischarge)
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

    public void Consume(ref float energy)
    {
        if (Stored >= energy)
        {
            Stored -= energy;
            energy = 0;
            return;
        }

        energy -= Math.Min(Stored, energy);
        Stored = 0;
    }

    public override string CompInspectStringExtra()
    {
        return $"Stored: {Stored:0.} / {Props.MaxStored:0.}";
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        dischargeTex ??= ContentFinder<Texture2D>.Get("ArchotechInfusions/UI/Gizmo/Discharge");
        yield return new Command_Toggle
        {
            defaultLabel = "JAI.Gizmo.Accumulator.MarkToDischarge".Translate(),
            defaultDesc = "JAI.Gizmo.Accumulator.MarkToDischarge.Desc".Translate(),
            icon = dischargeTex,
            hotKey = KeyBindingDefOf.Command_ColonistDraft,
            isActive = () => _markedToDischarge,
            toggleAction = () => _markedToDischarge = !_markedToDischarge
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "DEV: Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "DEV: -10%", action = () => Stored -= Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: +10%", action = () => Stored += Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: Full", action = () => Stored = Props.MaxStored };
        }
    }
}
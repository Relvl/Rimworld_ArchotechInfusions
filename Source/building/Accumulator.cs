using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.building;

[StaticConstructorOnStartup]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Accumulator : AGridBuilding
{
    private static readonly Vector2 BarSize = new(1.3f, 0.4f);
    private static readonly Material AccumulatorBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.85f, 0.85f));
    private static readonly Material AccumulatorBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));
    private static Texture2D _dischargeTex;

    private AccumulatorProps.Comp _comp;

    private bool _markedToDischarge;
    private float _stored;
    public AccumulatorProps.Comp Comp => _comp ??= GetComp<AccumulatorProps.Comp>();

    public float Stored
    {
        get => _stored;
        set => _stored = Math.Max(0, Math.Min(value, Comp.Props.MaxStored));
    }

    public bool MarkedToDischarge => _markedToDischarge;

    public bool IsFull => Stored >= Comp.Props.MaxStored;

    public CompPowerTrader Power { get; private set; }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Power = GetComp<CompPowerTrader>();
        if (Power is null)
            throw new Exception($"ArchInf: {GetType().FullName} can't work without {nameof(CompPowerTrader)}");
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = DrawPos + Vector3.up * 0.1f,
            size = BarSize,
            fillPercent = Stored / Comp.Props.MaxStored,
            filledMat = AccumulatorBarFilledMat,
            unfilledMat = AccumulatorBarUnfilledMat,
            margin = 0.15f,
            rotation = Rotation.CopyAndRotate(RotationDirection.Clockwise)
        });
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _stored, nameof(Stored));
        Scribe_Values.Look(ref _markedToDischarge, "MarkedToDischarge");
        Stored = _stored;
    }

    public override void Tick()
    {
        base.Tick();
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
    }

    protected override void FillInspectStringExtra(StringBuilder sb)
    {
        base.FillInspectStringExtra(sb);
        if (!Power.PowerOn)
            sb.AppendLine("JAI.Error.PoweredOff".Translate());
        sb.AppendLine($"Stored: {Stored:0.} / {Comp.Props.MaxStored:0.}");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        _dischargeTex ??= ContentFinder<Texture2D>.Get("ArchotechInfusions/UI/Gizmo/Discharge");

        yield return new Command_Toggle
        {
            defaultLabel = "JAI.Gizmo.Accumulator.MarkToDischarge".Translate(),
            defaultDesc = "JAI.Gizmo.Accumulator.MarkToDischarge.Desc".Translate(),
            icon = _dischargeTex,
            hotKey = KeyBindingDefOf.Command_ColonistDraft,
            isActive = () => _markedToDischarge,
            toggleAction = () => _markedToDischarge = !_markedToDischarge
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "DEV: Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "DEV: -10%", action = () => Stored -= Comp.Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: +10%", action = () => Stored += Comp.Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: Full", action = () => Stored = Comp.Props.MaxStored };
        }
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
}
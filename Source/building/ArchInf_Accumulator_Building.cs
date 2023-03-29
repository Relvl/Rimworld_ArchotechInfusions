using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.building;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global -- reflective
[StaticConstructorOnStartup]
public class ArchInf_Accumulator_Building : AddInf_Building
{
    private static readonly Vector2 BarSize = new(1.3f, 0.4f);
    private static readonly Material BarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.85f, 0.85f));
    private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private CompAccumulator _comp;

    private CompAccumulator Comp => _comp ??= GetComp<CompAccumulator>();

    public override void Draw()
    {
        base.Draw();
        DrawEnergyBar();
    }

    protected virtual void DrawEnergyBar()
    {
        var rotation = Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        var request = new GenDraw.FillableBarRequest
        {
            center = DrawPos + Vector3.up * 0.1f,
            size = BarSize,
            fillPercent = Comp.Stored / Comp.Props.MaxStored,
            filledMat = BarFilledMat,
            unfilledMat = BarUnfilledMat,
            margin = 0.15f,
            rotation = rotation
        };

        GenDraw.DrawFillableBar(request);
    }
}
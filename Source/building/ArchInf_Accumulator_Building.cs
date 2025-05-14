using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
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

    private Comp_Accumulator _comp;

    private Comp_Accumulator Comp => _comp ??= GetComp<Comp_Accumulator>();

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);

        var rotation = Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = DrawPos + Vector3.up * 0.1f,
            size = BarSize,
            fillPercent = Comp.Stored / Comp.Props.MaxStored,
            filledMat = BarFilledMat,
            unfilledMat = BarUnfilledMat,
            margin = 0.15f,
            rotation = rotation
        });
    }
}
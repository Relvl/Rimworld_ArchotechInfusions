using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.building;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global -- reflective
[StaticConstructorOnStartup]
public class ArchInf_Container_Building : AddInf_Building
{
    private static readonly Vector3 Offset = Vector3.up * 0.1f + Vector3.back * 0.35f;
    private static readonly Vector2 FuelBarSize = new(0.75f, 0.2f);
    private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));
    private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private Comp_ArchiteContainer _comp;

    private Comp_ArchiteContainer Comp => _comp ??= GetComp<Comp_ArchiteContainer>();

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = DrawPos + Offset,
            size = FuelBarSize,
            fillPercent = Comp.StoredPercent,
            filledMat = FuelBarFilledMat,
            unfilledMat = FuelBarUnfilledMat,
            margin = 0.15f,
            rotation = Rotation.CopyAndRotate(RotationDirection.Clockwise)
        });
    }
}
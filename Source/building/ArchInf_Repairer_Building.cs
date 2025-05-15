using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using UnityEngine;

namespace ArchotechInfusions.building;

public class ArchInf_Repairer_Building : AddInf_Building
{
    private Comp_ArchiteRepairer _repairerComp;
    public Comp_ArchiteRepairer RepairerComp => _repairerComp ??= GetComp<Comp_ArchiteRepairer>();

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
    }
}
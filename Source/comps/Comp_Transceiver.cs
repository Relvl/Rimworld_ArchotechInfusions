using System.Collections.Generic;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

public class CompProps_Transceiver : CompPropertiesBase_Grid
{
    public float ReceivePowerGain;
    public int ReceiveTicks;
    public float TransmitPowerGain;
    public int TransmitTicks;

    public CompProps_Transceiver()
    {
        compClass = typeof(Comp_Transceiver);
    }
}

public class Comp_Transceiver : CompBase_GridState<Comp_Transceiver, CompProps_Transceiver>
{
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;
    }
}
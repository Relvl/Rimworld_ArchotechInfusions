using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
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

public class Comp_Transceiver : CompBase_Grid<CompProps_Transceiver>;
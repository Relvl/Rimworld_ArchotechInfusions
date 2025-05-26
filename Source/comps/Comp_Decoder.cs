using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class CompProps_Decoder : CompPropertiesBase_Grid
{
    public IntRange DecodeTicks;
    public float DecodingEnergyGain;

    public float IdleEnergyGain;
    public float StartupEnergyGain;
    public int StartupTicks;

    public CompProps_Decoder()
    {
        compClass = typeof(Comp_Decoder);
    }
}

public class Comp_Decoder : CompBase_Grid<CompProps_Decoder>;
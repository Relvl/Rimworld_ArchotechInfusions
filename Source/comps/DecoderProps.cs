using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class DecoderProps : CompProperties
{
    public IntRange DecodeTicks;
    public float DecodingEnergyGain;

    public float IdleEnergyGain;
    public float StartupEnergyGain;
    public int StartupTicks;

    public DecoderProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<DecoderProps>;
}
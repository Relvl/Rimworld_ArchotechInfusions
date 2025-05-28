using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class KeyGeneratorProps : CompProperties
{
    public int AccumulatorRecacheTicks = 60;
    public int MaxStoredKeys = 3;
    public int TotalEnergyCost = 200;
    public int WorkAmount = 1500;

    public KeyGeneratorProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<KeyGeneratorProps>;
}
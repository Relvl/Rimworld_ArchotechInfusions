using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class CompProps_KeyGenerator : CompPropertiesBase_Grid
{
    public int AccumulatorRecacheTicks = 60;
    public int MaxStoredKeys = 3;
    public int TotalEnergyCost = 200;
    public int WorkAmount = 1500;

    public CompProps_KeyGenerator()
    {
        compClass = typeof(Comp_KeyGenerator);
    }
}

public class Comp_KeyGenerator : CompBase_Grid<CompProps_KeyGenerator>;
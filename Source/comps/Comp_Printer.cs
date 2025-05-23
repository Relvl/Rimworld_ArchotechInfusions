using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class CompProps_Printer : CompPropertiesBase_Grid
{
    public int PrintArchiteCost = 250;
    public int PrintEnergyCost = 1000;
    public int PrintTicks = 1000;

    public CompProps_Printer()
    {
        compClass = typeof(Comp_Printer);
    }
}

public class Comp_Printer : CompBase_Grid<CompProps_Printer>;
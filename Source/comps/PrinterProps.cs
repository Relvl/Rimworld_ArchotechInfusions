using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class PrinterProps : CompProperties
{
    public int PrintArchiteCost = 250;
    public int PrintEnergyCost = 1000;
    public int PrintTicks = 1000;

    public PrinterProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<PrinterProps>;
}
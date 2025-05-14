using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Printer : CompProperties
{
    public int PrintTicks = 1000;
    public int PrintEnergyCost = 1000;

    public CompProps_Printer() => compClass = typeof(Comp_Printer);
}

public class Comp_Printer : CompBase_Grid<CompProps_Printer>;
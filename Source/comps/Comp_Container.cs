using System.Collections.Generic;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Container : CompProperties
{
    public int MaxStored = 10000;
    public List<ItemData> AvailableItems = new();

    public CompProps_Container() => compClass = typeof(Comp_Container);
}

public class Comp_Container : CompBase_Membered<CompProps_Container>
{
}

public struct ItemData
{
    public ThingDef ThingDef;
    public int ArchiteCount;
}
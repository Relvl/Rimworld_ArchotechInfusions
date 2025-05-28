using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class ArchiteContainerProps : CompProperties
{
    public List<ItemData> AvailableItems = [];
    public int MaxStored = 10000;

    public ArchiteContainerProps()
    {
        compClass = typeof(Comp);
    }

    public class ItemData
    {
        public int ArchiteCount;
        public ThingDef ThingDef;
    }

    public class Comp : AGridComp<ArchiteContainerProps>;
}
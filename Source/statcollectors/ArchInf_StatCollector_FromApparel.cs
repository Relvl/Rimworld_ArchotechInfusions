using System.Collections.Generic;
using Verse;

namespace ArchotechInfusions.statcollectors;

// ReSharper disable once InconsistentNaming, UnusedType.Global - reflective ArchotechInfusions.statcollectors.StatCollectionElement.MakeStatCache
public class ArchInf_StatCollector_FromApparel : IStatCollector
{
    public IEnumerable<StatCollectionElement> Collect()
    {
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
        {
            if (!thingDef.IsApparel) continue;

            if (thingDef.statBases is not null)
                foreach (var modifier in thingDef.statBases)
                    yield return new StatCollectionElement(modifier.stat).FillThing(thingDef).FillOffsets(modifier).UpdateTypeFilter(InstructionTarget.Apparel);

            if (thingDef.equippedStatOffsets is not null)
                foreach (var modifier in thingDef.equippedStatOffsets)
                    yield return new StatCollectionElement(modifier.stat).FillThing(thingDef).FillOffsets(modifier).UpdateTypeFilter(InstructionTarget.Apparel);
        }
    }
}
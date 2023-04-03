using System.Collections.Generic;

namespace ArchotechInfusions.statcollectors;

public interface IStatCollector
{
    IEnumerable<StatCollectionElement> Collect();
}
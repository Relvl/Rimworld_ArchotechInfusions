using Verse;

namespace ArchotechInfusions.comps.comp_base;

public interface IGridComp<out TProperties> where TProperties : CompProperties
{
    TProperties Props { get; }
}
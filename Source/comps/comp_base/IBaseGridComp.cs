using Verse;

namespace ArchotechInfusions.comps.comp_base;

public interface IBaseGridComp<out TProps> where TProps : CompPropertiesBase_Grid
{
    Grid Grid { get; set; }

    ThingWithComps Parent { get; }

    TProps Props { get; }
}
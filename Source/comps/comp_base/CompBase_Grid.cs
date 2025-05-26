using System.Diagnostics.CodeAnalysis;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class CompBase_Grid<TProperties> : ThingComp where TProperties : CompPropertiesBase_Grid
{
    public TProperties Props => props as TProperties;
}
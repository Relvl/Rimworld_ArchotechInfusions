using System.Diagnostics.CodeAnalysis;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class AGridComp<TProperties> : ThingComp, IGridComp<TProperties> where TProperties : CompProperties
{
    public TProperties Props => props as TProperties;
}
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class CompBase_Grid<TProperties> : ThingComp, IBaseGridComp<TProperties> where TProperties : CompPropertiesBase_Grid
{
    private CompPowerTrader _power;
    public CompPowerTrader Power => _power ??= Parent.TryGetComp<CompPowerTrader>();

    public TProperties Props => props as TProperties;
    public Grid Grid { get; set; }
    public ThingWithComps Parent => parent;
}
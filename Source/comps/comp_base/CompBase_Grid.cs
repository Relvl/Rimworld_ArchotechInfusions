using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public class CompBase_Grid<T> : ThingComp where T : CompProperties
{
    private CompPowerTrader _power;
    private GridMemberComp _member;

    public CompPowerTrader Power => _power ??= parent.TryGetComp<CompPowerTrader>();
    public GridMemberComp Member => _member ??= parent.TryGetComp<GridMemberComp>();
    public Grid Grid => Member.Grid;
    public T Props => props as T;
}
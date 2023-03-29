using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable once InconsistentNaming
public class CompBase_Membered<T> : ThingComp where T : CompProperties
{
    private CompPowerTrader _power;
    private GridMemberComp _member;

    public CompPowerTrader Power => _power ??= parent.TryGetComp<CompPowerTrader>();
    public GridMemberComp Member => _member ??= parent.TryGetComp<GridMemberComp>();
    public T Props => props as T;
}
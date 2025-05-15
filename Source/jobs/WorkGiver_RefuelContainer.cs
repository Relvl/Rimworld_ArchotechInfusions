using System.Collections.Generic;
using ArchotechInfusions.comps;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

public class WorkGiver_RefuelContainer : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (var container in pawn.Map.ArchInfGrid().GetComps<Comp_ArchiteContainer>())
            if (container.CanStoreMore())
                yield return container.parent;
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Building) return false;
        if (pawn.Faction != Faction.OfPlayer) return false;
        if (t.Faction != pawn.Faction) return false;
        if (t.IsForbidden(pawn)) return false;
        if (t.IsBurning()) return false;
        if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null) return false;
        if (!pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), ignoreOtherReservations: forced)) return false;

        var comp = t.TryGetComp<Comp_ArchiteContainer>();
        if (!comp.CanStoreMore())
        {
            JobFailReason.Is("CantStoreMore".Translate());
            return false;
        }

        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var targetB = FindFuel(pawn, t.TryGetComp<Comp_ArchiteContainer>());
        if (targetB is null)
            return null;
        return JobMaker.MakeJob(JobDriverDefOf.ArchInf_RefuelContainer, t, targetB);
    }

    private static Thing FindFuel(Pawn pawn, Comp_ArchiteContainer architeContainer)
    {
        return GenClosest.ClosestThingReachable(
            pawn.Position,
            pawn.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
            PathEndMode.ClosestTouch,
            TraverseParms.For(pawn),
            validator: Validator,
            customGlobalSearchSet: architeContainer.GetAvailableFuels(pawn.Map)
        );

        bool Validator(Thing x)
        {
            if (x.IsForbidden(pawn)) return false;
            if (!pawn.CanReserveAndReach(x, PathEndMode.Touch, pawn.NormalMaxDanger())) return false;
            if (!architeContainer.IsThingAllowedAsFuel(x)) return false;
            return true;
        }
    }
}
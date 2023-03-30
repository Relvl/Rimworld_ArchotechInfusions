using System.Collections.Generic;
using ArchotechInfusions.comps;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

// ReSharper disable UnusedType.Global,InconsistentNaming -- def reflective
public class WorkGiver_RefuelContainer : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (var generator in pawn.Map.LightGrid().GetComps<Comp_Container>())
            if (generator.CanStoreMore())
                yield return generator.parent;
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

        var comp = t.TryGetComp<Comp_Container>();
        if (!comp.CanStoreMore())
        {
            JobFailReason.Is("CantStoreMore".Translate());
            return false;
        }

        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var targetB = FindFuel(pawn, t.TryGetComp<Comp_Container>());
        if (targetB is null) return null;
        return JobMaker.MakeJob(JobDriverDefOf.ArchInf_RefuelContainer, t, targetB);
    }

    private static Thing FindFuel(Pawn pawn, Comp_Container container)
    {
        bool Validator(Thing x)
        {
            if (x.IsForbidden(pawn)) return false;
            if (!pawn.CanReserveAndReach(x, PathEndMode.Touch, pawn.NormalMaxDanger())) return false;
            if (!container.IsThingAllowedAsFuel(x)) return false;
            return true;
        }

        return GenClosest.ClosestThingReachable(
            pawn.Position,
            pawn.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
            PathEndMode.ClosestTouch,
            TraverseParms.For(pawn),
            validator: Validator,
            customGlobalSearchSet: container.GetAvailableFuels(pawn.Map)
        );
    }
}

public class JobDriver_RefuelContainer : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);

        var comp = TargetA.Thing.TryGetComp<Comp_Container>();

        this.FailOn(() => comp is null || !comp.CanStoreMore());

        yield return Toils_Reserve.Reserve(TargetIndex.A);

        var reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveFuel;

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch) //
            .FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

        // todo! should we drop carryed thing?
        yield return Toils_Haul.StartCarryThing(TargetIndex.B) //
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);

        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        yield return Toils_General.Wait(200) //
            .FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A)
            .WithProgressBarToilDelay(TargetIndex.A);

        yield return new Toil
        {
            initAction = () =>
            {
                var thing = job.GetTarget(TargetIndex.B).Thing;
                comp.InsertFuel(thing);
                pawn.carryTracker.innerContainer.Remove(thing);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}
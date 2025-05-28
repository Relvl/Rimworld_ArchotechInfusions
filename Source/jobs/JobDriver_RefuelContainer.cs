using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class JobDriver_RefuelContainer : JobDriver
{
    private Thing FuelThing => job.GetTarget(TargetIndex.B).Thing;
    private ArchiteContainer Container => TargetA.Thing as ArchiteContainer;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)
               && pawn.Reserve(FuelThing, job, errorOnFailed: true);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);

        this.FailOn(() => Container is null || !Container.CanStoreMore());

        yield return Toils_Reserve.Reserve(TargetIndex.A);

        var reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveFuel;

        yield return Toils_General.DoAtomic(() => job.count = Container.CountToFullyRefuel(FuelThing));

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch) //
            .FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

        // todo! should we drop currently carried thing?
        yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true) //
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);

        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, true);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        yield return Toils_General.Wait(200) //
            .FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A);

        var finalToil = ToilMaker.MakeToil(nameof(JobDriver_RefuelContainer));
        finalToil.defaultCompleteMode = ToilCompleteMode.Instant;
        finalToil.initAction = () =>
        {
            var thing = job.GetTarget(TargetIndex.B).Thing;
            Container.InsertFuel(thing);
            pawn.carryTracker.innerContainer.Remove(thing);
        };
        yield return finalToil;
    }
}
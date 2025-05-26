using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class JobDriver_Print : JobDriver
{
    private ArchInf_Printer_Building Printer => TargetA.Thing as ArchInf_Printer_Building;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.ReserveSittableOrSpot(job.targetA.Thing.InteractionCell, job, errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => TargetA.Thing is not ArchInf_Printer_Building || Printer is null);
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);

        // Finish action. Not at last toil, because sometimes, e.g. interrupted, it will no call finish actions...
        AddFinishAction(condition => Printer.DoJobReallyFinished(condition));

        yield return Toils_Reserve.Reserve(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);

        var openInterface = Toils_General.DoAtomic(() => Printer.OpenSelectThingWindow(pawn));
        yield return openInterface;

        // todo thing about no delay, and waiting for window closed?..
        var someDelay = Toils_General.WaitWith(TargetIndex.A, 10);
        yield return someDelay;

        var workToil = ToilMaker.MakeToil();
        workToil.defaultCompleteMode = ToilCompleteMode.Never;
        workToil.activeSkill = () => SkillDefOf.Intellectual;
        workToil.WithEffect(EffecterDefOf.Research, TargetIndex.A);
        workToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        workToil.FailOn(() => !Printer.CanWork());
        workToil.initAction = () => Printer.DoJobStarted(this);
        workToil.tickAction = () => Printer.DoJobTick(this, pawn);
        workToil.WithProgressBar(TargetIndex.A, () => Printer.GetPercentComplete());
        yield return workToil;
    }
}
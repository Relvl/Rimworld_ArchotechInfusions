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
    private ArchInf_Printer_Building Building => TargetA.Thing as ArchInf_Printer_Building;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.ReserveSittableOrSpot(job.targetA.Thing.InteractionCell, job, errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => TargetA.Thing is not ArchInf_Printer_Building || Building.PrinterComp is null);
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);

        yield return Toils_Reserve.Reserve(TargetIndex.A);

        // go to the printer
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);

        // open print window
        var openInterface = Toils_General.DoAtomic(() => Building.PrinterComp.OpenSelectThingWindow(pawn, this));
        yield return openInterface;

        // just some delay
        yield return Toils_General.WaitWith(TargetIndex.A, 20);

        // work cycles
        var workToil = ToilMaker.MakeToil();
        workToil.defaultCompleteMode = ToilCompleteMode.Never;
        workToil.activeSkill = () => SkillDefOf.Intellectual;
        workToil.WithEffect(EffecterDefOf.Research, TargetIndex.A);
        workToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        workToil.FailOn(() => !Building.PrinterComp.CanWork());
        workToil.initAction = () => Building.PrinterComp.DoJobStarted(pawn, this, true);
        workToil.tickAction = () => Building.PrinterComp.DoJobTick(pawn, this, pawn.GetStatValue(StatDefOf.WorkSpeedGlobal));
        workToil.AddFinishAction(() => Building.PrinterComp.DoJobFinished(pawn, this));
        workToil.WithProgressBar(TargetIndex.A, () => Building.PrinterComp.GetPercentComplete());
        yield return workToil;

        yield return Toils_General.Do(() => Log.Warning("-- last toil"));
    }
}
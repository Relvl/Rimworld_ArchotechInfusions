using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class JobDriver_ArchiteRepair : JobDriver
{
    private ArchInf_Repairer_Building Repairer => TargetA.Thing as ArchInf_Repairer_Building;


    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.ReserveSittableOrSpot(job.targetA.Thing.InteractionCell, job, errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        var toil = ToilMaker.MakeToil();
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.activeSkill = () => SkillDefOf.Crafting;
        toil.WithEffect(EffecterDefOf.Research, TargetIndex.A);
        toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        toil.FailOn(() => !Repairer.CanWork());
        toil.tickAction = () => Repairer.DoJobTick(GetActor(), this, pawn.GetStatValue(StatDefOf.WorkSpeedGlobal));
        toil.WithProgressBar(TargetIndex.A, () => Repairer.GetPercentComplete());

        yield return toil;

        yield return Toils_General.Wait(2);
    }
}
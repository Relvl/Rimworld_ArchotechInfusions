using System.Collections.Generic;
using ArchotechInfusions.comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

// ReSharper disable UnusedType.Global,InconsistentNaming -- def reflective
public class JobDriver_GenerateKey : JobDriver
{
    private Comp_KeyGenerator _keyGenerator;

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
        toil.activeSkill = () => SkillDefOf.Intellectual;
        toil.WithEffect(EffecterDefOf.Research, TargetIndex.A);
        toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        toil.FailOn(() => Generator is null || !Generator.CanGenerateNewKey());
        toil.tickAction = () => Generator.DoGenerateTick(GetActor(), this, pawn.GetStatValue(StatDefOf.ResearchSpeed));
        toil.WithProgressBar(TargetIndex.A, () => TargetA.Thing.TryGetComp<Comp_KeyGenerator>()?.GetPercentComplete() ?? 0f);
        yield return toil;

        yield return Toils_General.Wait(2);
    }

    private Comp_KeyGenerator Generator => _keyGenerator ??= TargetA.Thing.TryGetComp<Comp_KeyGenerator>();
}
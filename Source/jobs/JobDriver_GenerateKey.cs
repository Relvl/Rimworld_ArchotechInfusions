using System.Collections.Generic;
using ArchotechInfusions.comps;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

// ReSharper disable UnusedType.Global,InconsistentNaming -- def reflective
public class WorkGiver_GenerateKey : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
    public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (var generator in pawn.Map.LightGrid().GetComps<Comp_KeyGenerator>())
            if (generator.CanGenerateNewKey())
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
        if (!pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)) return false;
        return new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(JobDriverDefOf.ArchInf_GenerateKey, t, 1500, true);
}

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
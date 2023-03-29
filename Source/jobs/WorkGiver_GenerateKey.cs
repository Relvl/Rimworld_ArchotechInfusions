using System.Collections.Generic;
using ArchotechInfusions.comps;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

// ReSharper disable once InconsistentNaming, UnusedType.Global -- def reflected
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
        if (!pawn.CanReserve(t, ignoreOtherReservations: forced)) return false;
        if (!pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)) return false;
        return new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(JobDriverDefOf.ArchInf_GenerateKey, t, 1500, true);
}
﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ArchotechInfusions.building;
using ArchotechInfusions.defOf;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.jobs;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class WorkGiver_ArchiteRepair : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (var repairer in pawn.Map.ArchInfGrid().Get<ArchiteRepairer>())
        {
            if (!repairer.CanWork()) continue;
            yield return repairer;
        }
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

        if (t is not ArchiteRepairer repairer) return false;
        var item = repairer.GetAllRepairableThings(pawn).FirstOrDefault();
        if (item == null)
        {
            JobFailReason.Is("JAI.Error.NoDamagedThings".Translate());
            return false;
        }

        return new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(JobDriverDefOf.ArchInf_RepairInventory, t, 1500, true);
    }
}
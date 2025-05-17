using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ArchInf_Printer_Building : AddInf_Building
{
    private Comp_Printer _printerComp;
    public Comp_Printer PrinterComp => _printerComp ??= GetComp<Comp_Printer>();


    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        foreach (var option in base.GetFloatMenuOptions(selPawn))
            yield return option;

        yield return new FloatMenuOption("Print stat instruction...", () => OrderStartPrinting(selPawn));
    }

    public void OrderStartPrinting(Pawn pawn)
    {
        pawn.jobs.TryTakeOrderedJob(new Job(JobDriverDefOf.ArchInf_Print, this, 1500), JobTag.Misc);
    }
}
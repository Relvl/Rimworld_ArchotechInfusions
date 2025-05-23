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

        if (!PrinterComp.HasEnoughPower())
        {
            yield return new FloatMenuOption("JAI.FloatMenu.Printer.NoPower".Translate(), null);
            yield break;
        }

        if (!PrinterComp.HasEnoughArchite())
        {
            yield return new FloatMenuOption("JAI.FloatMenu.Printer.NoArchite".Translate(), null);
            yield break;
        }

        if (!PrinterComp.HasAnyInstruction())
        {
            yield return new FloatMenuOption("JAI.FloatMenu.Printer.NoInstructions".Translate(), null);
            yield break;
        }

        yield return new FloatMenuOption("JAI.FloatMenu.Printer.Start".Translate(), () => OrderStartPrinting(selPawn));
    }

    public void OrderStartPrinting(Pawn pawn)
    {
        pawn.jobs.TryTakeOrderedJob(new Job(JobDriverDefOf.ArchInf_Print, this, 1500), JobTag.Misc);
    }
}
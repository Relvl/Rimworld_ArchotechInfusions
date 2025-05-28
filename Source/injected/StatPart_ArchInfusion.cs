using System.Diagnostics.CodeAnalysis;
using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.injected;

/// <summary>
///     Common stat part that applies instructions
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class StatPart_ArchInfusion : StatPart
{
    public StatPart_ArchInfusion(StatDef parentStat)
    {
        this.parentStat = parentStat;
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.HasThing)
            return;

        if (req.Thing is Pawn pawn && req.Thing.def.race is not null)
        {
            if (pawn.equipment?.Primary is not null)
                if (pawn.equipment.Primary.TryGetInfusedComp(out var compPrimary))
                    TransformValueInner(compPrimary, ref val, pawn);

            if (pawn.apparel?.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                    if (apparel.TryGetInfusedComp(out var compApparel))
                        TransformValueInner(compApparel, ref val, pawn);
        }

        if (req.Thing.TryGetInfusedComp(out var compDirect))
            TransformValueInner(compDirect, ref val, req.Thing);
    }

    private void TransformValueInner(InstructionsComps comp, ref float value, Thing requester)
    {
        foreach (var instruction in comp.Instructions)
            instruction.TransformStatValue(parentStat, ref value, requester);
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!req.HasThing)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine("JAI.statpart.title".Translate());

        var anyChanged = false;

        if (req.Thing is Pawn pawn && req.Thing.def.race is not null)
        {
            // todo blacklisted stats? maybe no, bcz there is fully configurable via defs?..
            if (pawn.equipment.Primary is not null)
                if (pawn.equipment.Primary.TryGetInfusedComp(out var compPrimary))
                    anyChanged |= ExplanationPartInner(compPrimary, sb, pawn);

            if (pawn.apparel.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                    if (apparel.TryGetInfusedComp(out var compApparel))
                        anyChanged |= ExplanationPartInner(compApparel, sb, pawn);
        }

        if (req.Thing.TryGetInfusedComp(out var compDirect))
            anyChanged |= ExplanationPartInner(compDirect, sb, req.Thing);

        if (!anyChanged) return null;

        return sb.ToString();
    }

    private bool ExplanationPartInner(InstructionsComps comp, StringBuilder sb, Thing requester)
    {
        var anyChanged = false;
        foreach (var instruction in comp.Instructions)
            anyChanged |= instruction.AddStatExplanation(parentStat, sb, requester);
        return anyChanged;
    }
}
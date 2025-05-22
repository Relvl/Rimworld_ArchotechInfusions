using System.Diagnostics.CodeAnalysis;
using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.injected;

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
            // todo blacklisted stats for pawns. some only apply to equipment.
            if (pawn.equipment?.Primary is not null)
                if (pawn.equipment.Primary.TryGetInfusedComp(out var compPrimary))
                    TransformValueInner(compPrimary, ref val);

            if (pawn.apparel?.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                    if (apparel.TryGetInfusedComp(out var compApparel))
                        TransformValueInner(compApparel, ref val);
        }

        if (req.Thing.TryGetInfusedComp(out var compDirect))
            TransformValueInner(compDirect, ref val);
    }

    private void TransformValueInner(Comp_ArchInfused comp, ref float value)
    {
        foreach (var instruction in comp.Instructions)
            instruction.TransformStatValue(parentStat, ref value);
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
                    anyChanged |= ExplanationPartInner(compPrimary, sb);

            if (pawn.apparel.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                    if (apparel.TryGetInfusedComp(out var compApparel))
                        anyChanged |= ExplanationPartInner(compApparel, sb);
        }

        if (req.Thing.TryGetInfusedComp(out var compDirect))
            anyChanged |= ExplanationPartInner(compDirect, sb);

        if (!anyChanged) return null;

        return sb.ToString();
    }

    private bool ExplanationPartInner(Comp_ArchInfused comp, StringBuilder sb)
    {
        var anyChanged = false;
        foreach (var instruction in comp.Instructions)
            anyChanged |= instruction.AddStatExplanation(parentStat, sb);
        return anyChanged;
    }
}
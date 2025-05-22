using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.injected;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class StatPart_ArchInfusion : StatPart
{
    private const string TAB = "\t";

    public StatPart_ArchInfusion(StatDef parentStat)
    {
        this.parentStat = parentStat;
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.HasThing) return;

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

    private static void TransformValueInner(Comp_ArchInfused comp, ref float value)
    {
        if (!comp.HasInstructions)
            return;
        foreach (var instruction in comp.Instructions)
            switch (instruction.Type)
            {
                case EInstructionType.Add:
                    value += instruction.Value;
                    break;
                case EInstructionType.Mul:
                    value *= instruction.Value;
                    break;
                case EInstructionType.Force:
                    value = instruction.Value;
                    break;
            }
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!req.HasThing)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine("JAI.statpart.title".Translate());

        if (req.Thing is Pawn pawn && req.Thing.def.race is not null)
        {
            // todo blacklisted stats? maybe no, bcz there is fully configurable via defs?..
            if (pawn.equipment.Primary is not null)
                if (pawn.equipment.Primary.TryGetInfusedComp(out var compPrimary))
                    ExplanationPartInner(compPrimary, sb);

            if (pawn.apparel.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                    if (apparel.TryGetInfusedComp(out var compApparel))
                        ExplanationPartInner(compApparel, sb);
        }

        if (req.Thing.TryGetInfusedComp(out var compDirect))
        {
            sb.Append(TAB).AppendLine("JAI.instruction.integrity.value".Translate(compDirect.Integrity.ToString("0.##")));
            if (compDirect.IsUnbreakable)
                sb.Append(TAB).AppendLine("JAI.instruction.unbreakable".Translate());
            ExplanationPartInner(compDirect, sb);
        }

        return sb.ToString();
    }

    private void ExplanationPartInner(Comp_ArchInfused comp, StringBuilder sb)
    {
        foreach (var instruction in comp.Instructions)
        {
            sb.Append(TAB).Append(instruction.Label);
            switch (instruction.Type)
            {
                case EInstructionType.Add:
                    sb.Append(instruction.Value > 0 ? ": +" : ": -").Append(parentStat.ValueToString(Mathf.Abs(instruction.Value))).AppendLine();
                    break;
                case EInstructionType.Mul:
                    sb.Append(": x").Append(instruction.Value).AppendLine();
                    break;
                case EInstructionType.Force:
                    sb.Append(instruction.Value > 0 ? ": =" : ": = -").Append(parentStat.ValueToString(Mathf.Abs(instruction.Value))).AppendLine();
                    break;
            }
        }
    }
}
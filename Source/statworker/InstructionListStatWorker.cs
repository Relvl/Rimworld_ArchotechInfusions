using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.defOf;
using ArchotechInfusions.injected;
using RimWorld;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionListStatWorker : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        if (!req.HasThing) return false;
        if (req.Thing is Pawn && req.Thing.def.race is not null) return true;
        return req.Thing.HasComp<InstructionsComps>();
    }

    public override bool IsDisabledFor(Thing thing)
    {
        if (thing is Pawn && thing.def.race is not null) return false;
        return !thing.def.IsApparel && !thing.def.IsWeapon;
    }


    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        if (!req.HasThing) return 0;

        if (req.Thing is Pawn pawn && req.Thing.def.race is not null)
        {
            var value = 0f;
            if (pawn.equipment?.Primary is not null)
                value += pawn.equipment.Primary.GetStatValue(ArchInfStatDefOf.JAI_Instructions_List);

            if (pawn.apparel?.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                    value += apparel.GetStatValue(ArchInfStatDefOf.JAI_Instructions_List);
            return value;
        }

        if (req.Thing.TryGetInfusedComp(out var comp))
            return comp.Instructions.Count + (comp.IsUnbreakable ? 1 : 0);

        return 0;
    }

    public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
    {
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        if (!req.HasThing) return "";
        var sb = new StringBuilder();

        if (req.Thing is Pawn pawn && req.Thing.def.race is not null)
        {
            if (pawn.equipment?.Primary is not null)
            {
                sb.Append(pawn.equipment.Primary.LabelCap).AppendLine(": ");
                sb.AppendLine(GetExplanationUnfinalized(StatRequest.For(pawn.equipment.Primary), numberSense));
                sb.AppendLine();
            }

            if (pawn.apparel?.WornApparel is not null)
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    sb.Append(apparel.LabelCap).AppendLine(": ");
                    sb.AppendLine(GetExplanationUnfinalized(StatRequest.For(apparel), numberSense));
                    sb.AppendLine();
                }
        }

        if (req.Thing.TryGetInfusedComp(out var comp))
        {
            if (comp.IsUnbreakable)
                sb.Append("JAI.Stat.Integrity.Unbreakable".Translate()).Append(", ")
                    .Append("JAI.instruction.complexity.value".Translate(ArchInfStatDefinitionDefOf.Unbreakable.Complexity));
            foreach (var instruction in comp.Instructions)
            {
                sb.Append(instruction.Label).Append(": ");
                instruction.RenderValue(sb);
                sb.Append(", ").AppendLine("JAI.instruction.complexity.value".Translate(instruction.Complexity));
            }
        }

        return sb.TrimEnd().ToString().Trim(' ', '\r', '\n', '\t');
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        return "";
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
    {
        yield break;
    }
}
using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionQuality : AInstruction
{
    public override string Label => "Quality".Translate();

    public override void RenderValue(StringBuilder sb) => sb.Append("<color=green>+1</color>");

    public override bool IsThingApplicable(Thing thing)
    {
        var result = base.IsThingApplicable(thing);
        if (result && thing.TryGetQuality(out var ql))
            return ql < QualityCategory.Legendary;
        return result;
    }
}
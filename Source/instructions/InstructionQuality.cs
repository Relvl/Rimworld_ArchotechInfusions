using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.injected;
using RimWorld;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionQuality(StatDefinitionDef definition, StatDefinitionDef.Operation operation) : AInstruction(definition, operation)
{
    /// <summary>
    ///     IExposable constructor
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private InstructionQuality() : this(default, null)
    {
    }

    public override string Label => "Quality".Translate();

    public override void RenderValue(StringBuilder sb)
    {
        sb.Append("<color=green>+1</color>");
    }

    public override bool IsThingApplicable(Thing thing, InstructionsComps comp)
    {
        var result = base.IsThingApplicable(thing, comp);
        if (result && thing.TryGetQuality(out var ql))
            return ql < QualityCategory.Legendary;
        return result;
    }

    protected override float GetComplexity()
    {
        return Definition.Complexity;
    }
}
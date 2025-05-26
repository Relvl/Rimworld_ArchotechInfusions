using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.injected;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionNop(StatDefinitionDef definition, StatDefinitionDef.Operation operation) : AInstruction(definition, operation)
{
    /// <summary>
    ///     IExposable constructor
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private InstructionNop() : this(default, null)
    {
    }

    public override string Label => "JAI.instruction.nop".Translate();
    public override Color BgColor => Color.black;

    protected override float GetComplexity()
    {
        return Definition.Complexity;
    }

    public override void RenderComplexity(StringBuilder sb)
    {
    }

    public override void RenderLabel(StringBuilder sb)
    {
        sb.Append("<color=grey>").Append(Label).Append("</color> ");
    }

    public override void RenderTooltip(StringBuilder sb)
    {
        sb.Append("JAI.instruction.nop.Desc".Translate());
    }

    public override void RenderValue(StringBuilder sb)
    {
    }

    public override bool IsThingApplicable(Thing thing, Comp_ArchInfused comp)
    {
        return false;
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.injected;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionUnbreakable(StatDefinitionDef definition, StatDefinitionDef.Operation operation) : AInstruction(definition, operation)
{
    private static readonly Color Bg = Color.blue.ToTransparent(0.3f);


    /// <summary>
    ///     IExposable constructor
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private InstructionUnbreakable() : this(default, null)
    {
    }

    public override string Label => "JAI.instruction.unbreakable".Translate();
    public override Color BgColor => Bg;

    public override void RenderValue(StringBuilder sb)
    {
    }

    public override void RenderExtraLine(StringBuilder sb)
    {
        sb.Append("<color=#E57AFF>Jackpot!</color>");
    }

    protected override float GetComplexity()
    {
        return Definition.Complexity;
    }

    public override bool IsThingApplicable(Thing thing, InstructionsComps comp)
    {
        return !comp.IsUnbreakable && base.IsThingApplicable(thing, comp);
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionIntegrity(StatDefinitionDef definition, StatDefinitionDef.Operation operation) : AInstruction(definition, operation)
{
    private static readonly Color Bg = Color.blue.ToTransparent(0.3f);

    /// <summary>
    ///     IExposable constructor
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private InstructionIntegrity() : this(default, null)
    {
    }

    public override string Label => "JAI.instruction.integrity".Translate();
    public override Color BgColor => Bg;

    public override void RenderComplexity(StringBuilder sb)
    {
    }

    public override void RenderLabel(StringBuilder sb)
    {
        sb.Append("<color=#00FF00>").Append(Label).Append("</color> ");
    }

    public override void RenderExtraLine(StringBuilder sb)
    {
        sb.Append("<color=#E57AFF>Jackpot!</color>");
    }
}
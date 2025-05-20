using System.Text;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionIntegrity : AInstruction
{
    private static Color Bg = Color.blue.ToTransparent(0.3f);
    
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
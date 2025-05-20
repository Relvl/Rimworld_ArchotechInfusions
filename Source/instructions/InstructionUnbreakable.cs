using System.Text;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionUnbreakable : AInstruction
{
    private static Color Bg = Color.blue.ToTransparent(0.3f);
    public override string Label => "JAI.instruction.unbreakable".Translate();
    public override Color BgColor => Bg;
    

    public override void RenderValue(StringBuilder sb)
    {
    }
    
    public override void RenderExtraLine(StringBuilder sb)
    {
        sb.Append("<color=#E57AFF>Jackpot!</color>");
    }
}
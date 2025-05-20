using System.Text;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionQuality : AInstruction
{
    public override string Label => "Quality".Translate();

    public override void FillValueString(StringBuilder sb) => sb.Append("<color=green>+1</color>");
}
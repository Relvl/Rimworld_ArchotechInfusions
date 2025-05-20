using System.Text;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionUnbreakable : AInstruction
{
    public override string Label => "JAI.instruction.unbreakable".Translate();

    public override void FillValueString(StringBuilder sb)
    {
    }
}
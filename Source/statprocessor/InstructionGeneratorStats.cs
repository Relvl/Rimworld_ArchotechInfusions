using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions;

public class InstructionGeneratorStats(StatDefinitionDef def) : AInstructionGenerator<InstructionStat>(def)
{
    public override string Name => Def.defName;

    protected override InstructionStat InstantiateInstruction()
    {
        return new InstructionStat(Def, Operations.RandomElementByWeight(i => i.Weight));
    }
}
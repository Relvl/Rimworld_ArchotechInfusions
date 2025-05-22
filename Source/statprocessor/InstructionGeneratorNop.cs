using ArchotechInfusions.instructions;

namespace ArchotechInfusions;

public class InstructionGeneratorNop(StatDefinitionDef def) : AInstructionGenerator<InstructionNop>(def)
{
    public override string Name => "NOP";

    protected override InstructionNop InstantiateInstruction()
    {
        return new InstructionNop(Def, Def.OperationSet);
    }

    protected override float GenerateValue(AInstruction instruction)
    {
        return 0;
    }
}
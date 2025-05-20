using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

public class InstructionProcessorStats(StatDefinitionDef def) : AInstructionProcessor<InstructionStat>(def)
{
    protected override InstructionStat InstantiateInstruction() => new();

    protected override void FillInstruction(InstructionStat instruction, StatDefinitionDef.Operation operation)
    {
        instruction.Def = Def.StatDef;
        base.FillInstruction(instruction, operation);
    }
}
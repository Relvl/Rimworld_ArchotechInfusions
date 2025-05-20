using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

public class InstructionProcessorDurability(StatDefinitionDef def) : AInstructionProcessor<InstructionDurability>(def)
{
    protected override InstructionDurability InstantiateInstruction() => new();
}
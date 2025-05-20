using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

public class InstructionProcessorWeight(StatDefinitionDef def) : AInstructionProcessor<InstructionWeight>(def)
{
    protected override InstructionWeight InstantiateInstruction() => new();
}
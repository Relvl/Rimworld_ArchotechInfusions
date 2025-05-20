using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

public class InstructionProcessorIntegrity(StatDefinitionDef def) : AInstructionProcessor<InstructionIntegrity>(def)
{
    protected override InstructionIntegrity InstantiateInstruction() => new();
}
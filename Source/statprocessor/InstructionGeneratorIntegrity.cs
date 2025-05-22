using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionGeneratorIntegrity(StatDefinitionDef def) : AInstructionGenerator<InstructionIntegrity>(def)
{
    protected override InstructionIntegrity InstantiateInstruction()
    {
        return new InstructionIntegrity(Def, Operations.RandomElementByWeight(i => i.Weight));
    }
}
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionGeneratorNop(StatDefinitionDef def) : AInstructionGenerator<InstructionNop>(def)
{
    public override string Name => "NOP";

    protected override InstructionNop InstantiateInstruction()
    {
        return new InstructionNop(Def, Operations.RandomElementByWeight(i => i.Weight));
    }

    protected override float GenerateValue(AInstruction instruction)
    {
        return 0;
    }
}
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionGeneratorUnbreakable(StatDefinitionDef def) : AInstructionGenerator<InstructionUnbreakable>(def)
{
    public override void Init()
    {
        if (Def.OperationSet is not null)
        {
            TotalWeight = Def.OperationSet.Weight;
            Operations.Add(Def.OperationSet);
        }
    }

    protected override InstructionUnbreakable InstantiateInstruction()
    {
        return new InstructionUnbreakable(Def, Operations.RandomElementByWeight(i => i.Weight));
    }

    protected override float GenerateValue(AInstruction instruction)
    {
        return 1f;
    }
}
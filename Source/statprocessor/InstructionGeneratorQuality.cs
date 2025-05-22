using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionGeneratorQuality(StatDefinitionDef def) : AInstructionGenerator<InstructionQuality>(def)
{
    public override void Init()
    {
        if (Def.OperationAdd is not null)
        {
            TotalWeight = Def.OperationAdd.Weight;
            Operations.Add(Def.OperationAdd);
        }
    }

    protected override InstructionQuality InstantiateInstruction()
    {
        return new InstructionQuality(Def, Operations.RandomElementByWeight(i => i.Weight));
    }

    protected override float GenerateValue(AInstruction instruction)
    {
        return 1f;
    }
}
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionProcessorUnbreakable(StatDefinitionDef def) : AInstructionProcessor<InstructionUnbreakable>(def)
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
        return new InstructionUnbreakable();
    }

    protected override float GenerateValue(InstructionUnbreakable instruction, StatDefinitionDef.Operation operation)
    {
        return 1f;
    }

    protected override float GenerateComplexity(InstructionUnbreakable instruction, StatDefinitionDef.Operation operation)
    {
        return Def.Complexity;
    }
}
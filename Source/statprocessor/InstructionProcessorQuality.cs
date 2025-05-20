using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.instructions;

namespace ArchotechInfusions.statprocessor;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class InstructionProcessorQuality(StatDefinitionDef def) : AInstructionProcessor<InstructionQuality>(def)
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
        return new InstructionQuality();
    }

    protected override float GenerateValue(InstructionQuality instruction, StatDefinitionDef.Operation operation)
    {
        return 1f;
    }

    protected override float GenerateComplexity(InstructionQuality instruction, StatDefinitionDef.Operation operation)
    {
        return Def.Complexity;
    }
}
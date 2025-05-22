using System.Collections.Generic;
using ArchotechInfusions.instructions;
using UnityEngine;

namespace ArchotechInfusions;

public abstract class AInstructionGenerator<T>(StatDefinitionDef def) : IInstructionProcessor<T> where T : AInstruction
{
    protected readonly List<StatDefinitionDef.Operation> Operations = [];
    protected StatDefinitionDef Def { get; } = def;

    public int TotalWeight { get; protected set; }

    public virtual string Name => GetType().Name;

    public virtual void Init()
    {
        if (Def.OperationAdd is not null)
        {
            TotalWeight += Def.OperationAdd.Weight;
            Operations.Add(Def.OperationAdd);
        }

        if (Def.OperationMul is not null)
        {
            TotalWeight += Def.OperationMul.Weight;
            Operations.Add(Def.OperationMul);
        }

        if (Def.OperationSet is not null)
        {
            TotalWeight += Def.OperationSet.Weight;
            Operations.Add(Def.OperationSet);
        }
    }

    public bool IsSpecial()
    {
        return Def.Worker != typeof(InstructionGeneratorStats);
    }

    public T GenerateInstruction()
    {
        var instruction = InstantiateInstruction();
        instruction.Value = GenerateValue(instruction);
        return instruction;
    }

    protected virtual float GenerateValue(AInstruction instruction)
    {
        float value;
        var watchdog = 10;
        if (instruction.Operation.IsInverted)
            do
            {
                value = Random.Range(instruction.Operation.Max, instruction.Operation.Min);
            } while (watchdog-- > 0 && !Mathf.Approximately(value, instruction.Operation.Default));
        else
            do
            {
                value = Random.Range(instruction.Operation.Min, instruction.Operation.Max);
            } while (watchdog-- > 0 && !Mathf.Approximately(value, instruction.Operation.Default));

        return value;
    }

    protected abstract T InstantiateInstruction();
}
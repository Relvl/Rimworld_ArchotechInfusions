using System.Collections.Generic;
using ArchotechInfusions.instructions;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace ArchotechInfusions.statprocessor;

public abstract class AInstructionProcessor<T>(StatDefinitionDef def) : IInstructionProcessor<T> where T : AInstruction
{
    protected readonly List<StatDefinitionDef.Operation> Operations = [];
    public StatDefinitionDef Def { get; } = def;

    public int TotalWeight { get; protected set; }

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

    public T GenerateInstruction()
    {
        var instruction = InstantiateInstruction();
        var operation = GetRandomOperation();
        FillInstruction(instruction, operation);
        return instruction;
    }

    private StatDefinitionDef.Operation GetRandomOperation()
    {
        return Operations.RandomElementByWeight(i => i.Weight);
    }

    protected abstract T InstantiateInstruction();

    protected virtual void FillInstruction(T instruction, StatDefinitionDef.Operation operation)
    {
        instruction.Type = operation.OperationType;
        instruction.TypeFilter = Def.Target;
        instruction.Value = GenerateValue(instruction, operation);
        instruction.Complexity = GenerateComplexity(instruction, operation);
    }

    protected virtual float GenerateValue(T instruction, StatDefinitionDef.Operation operation)
    {
        float value;
        var watchdog = 10;
        if (operation.isInverted)
            do
            {
                value = Random.Range(operation.Max, operation.Min);
            } while (watchdog-- > 0 && !Mathf.Approximately(value, operation.Default));
        else
            do
            {
                value = Random.Range(operation.Min, operation.Max);
            } while (watchdog-- > 0 && !Mathf.Approximately(value, operation.Default));

        return value;
    }

    protected virtual float GenerateComplexity(T instruction, StatDefinitionDef.Operation operation)
    {
        // don't make the insane math anymore please... it makes me sick. old good conditions is more clear.
        float factor;
        // lesser is better and uses more complexity
        if (operation.isInverted)
        {
            // is "positive" effect and more complexity
            if (instruction.Value <= operation.Default)
            {
                factor = (instruction.Value - operation.Default) / (operation.Max - operation.Default);
            }
            // is "negative" effect and lowers the complexity
            else
            {
                factor = (instruction.Value - operation.Default) / (operation.Default - operation.Min);
            }
        }
        // greater is better and uses more complexity
        else
        {
            // is "positive" effect and more complexity
            if (instruction.Value >= operation.Default)
            {
                factor = (instruction.Value - operation.Default) / (operation.Max - operation.Default);
            }
            // is "negative" effect and lowers the complexity
            else
            {
                factor = (instruction.Value - operation.Default) / (operation.Default - operation.Min);
            }
        }

        return Mathf.Lerp(-Def.Complexity / /*todo config? def? */10f, Def.Complexity, (factor + 1f) / 2f);
    }
}
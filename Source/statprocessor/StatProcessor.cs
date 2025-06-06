﻿using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions;

public static class StatProcessor
{
    private static List<IInstructionProcessor<AInstruction>> _processors;
    private static List<IInstructionProcessor<AInstruction>> Processors => _processors ??= Init().ToList();

    private static IEnumerable<IInstructionProcessor<AInstruction>> Init()
    {
        foreach (var statDefinitionDef in DefDatabase<StatDefinitionDef>.AllDefs)
        {
            if (Activator.CreateInstance(statDefinitionDef.Worker, statDefinitionDef) is not IInstructionProcessor<AInstruction> processor)
            {
                Log.Error($"Failed to instantiate stat processor {statDefinitionDef.Worker}");
                continue;
            }

            // todo some checks
            processor.Init();
            yield return processor;
        }
    }

    private static IInstructionProcessor<AInstruction> GetRandomProcessor()
    {
        return Processors.RandomElementByWeight(p => p.TotalWeight);
    }

    public static AInstruction GenerateInstruction()
    {
        return GetRandomProcessor().GenerateInstruction();
    }

    public static IEnumerable<IInstructionProcessor<AInstruction>> GetSpecialGenerators()
    {
        return Processors.Where(p => p.IsSpecial());
    }

    public static IEnumerable<InstructionGeneratorStats> GetStatGenerators()
    {
        return Processors.Where(p => p is InstructionGeneratorStats).Cast<InstructionGeneratorStats>();
    }
}
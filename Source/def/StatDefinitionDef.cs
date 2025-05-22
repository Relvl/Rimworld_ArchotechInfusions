using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.injected;
using ArchotechInfusions.statprocessor;
using RimWorld;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class StatDefinitionDef : Def
{
    /// <summary>
    /// 
    /// </summary>
    public StatDef StatDef;

    /// <summary>
    /// 
    /// </summary>
    public Type Worker = typeof(InstructionProcessorStats);

    /// <summary>
    /// 
    /// </summary>
    public EInstructionTarget Target = EInstructionTarget.Any;

    /// <summary>
    /// 
    /// </summary>
    public float Complexity = 100f;

    /// <summary>
    /// 
    /// </summary>
    public ExtraArchiteData ExtraArchite;

    /// <summary>
    /// 
    /// </summary>
    public Operation OperationAdd;

    /// <summary>
    /// 
    /// </summary>
    public Operation OperationMul;

    /// <summary>
    /// 
    /// </summary>
    public Operation OperationSet;

    public override void PostSetIndices()
    {
        if (OperationAdd is not null) OperationAdd.OperationType = EInstructionType.Add;
        if (OperationMul is not null) OperationMul.OperationType = EInstructionType.Mul;
        if (OperationSet is not null) OperationSet.OperationType = EInstructionType.Force;
    }

    public override void ResolveReferences()
    {
        if (StatDef is null) return;
        StatDef.parts ??= [];
        if (StatDef.parts.Any(part => part.GetType() == typeof(StatPart_ArchInfusion))) return;
        if (StatDef.parts.Any(part => part.GetType() == typeof(StatPart_ExtraMarketValue))) return;
        StatDef.parts.Add(new StatPart_ArchInfusion(StatDef));
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
            yield return error;

        if (StatDef is not null && Worker != typeof(InstructionProcessorStats))
            yield return $"JAI: StatDefinitionDef[defName={defName}]/StatDef is not null && Worker != InstructionProcessorStats";

        if (OperationAdd is null && OperationMul is null && OperationSet is null)
            yield return $"JAI: StatDefinitionDef[defName={defName}] has no operation add/mul/set defined";

        if (OperationAdd is not null)
        {
            if (OutOfRange(OperationAdd))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationAdd Default is out of Min..Max";
            if (Mathf.Approximately(OperationAdd.Min, OperationAdd.Default) && Mathf.Approximately(OperationAdd.Max, OperationAdd.Default))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationAdd Default==Min==Max";
        }

        if (OperationMul is not null)
        {
            if (OutOfRange(OperationMul))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationMul Default is out of Min..Max";
            if (Mathf.Approximately(OperationMul.Min, OperationMul.Default) && Mathf.Approximately(OperationMul.Max, OperationMul.Default))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationMul Default==Min==Max";
        }

        if (OperationSet is not null)
        {
            if (OutOfRange(OperationSet))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationSet Default is out of Min..Max";
            if (Mathf.Approximately(OperationSet.Min, OperationSet.Default) && Mathf.Approximately(OperationSet.Max, OperationSet.Default))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationSet Default==Min==Max";
        }

        if (OperationAdd is not null && OperationSet is not null && OperationAdd.isInverted != OperationSet.isInverted)
            yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationAdd.isInverted != OperationSet.isInverted";
        if (OperationMul is not null && OperationSet is not null && OperationMul.isInverted != OperationSet.isInverted)
            yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationMul.isInverted != OperationSet.isInverted";
        if (OperationAdd is not null && OperationMul is not null && OperationAdd.isInverted != OperationMul.isInverted)
            yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationAdd.isInverted != OperationMul.isInverted";
    }

    private static bool OutOfRange(Operation operation)
    {
        if (operation.Max > operation.Min && (operation.Default < operation.Min || operation.Default > operation.Max)) return true;
        if (operation.Min > operation.Max && (operation.Default < operation.Max || operation.Default > operation.Min)) return true;
        return false;
    }

    public class Operation
    {
        /// <summary>
        /// 
        /// </summary>
        public int Weight = 1;

        /// <summary>
        /// 
        /// </summary>
        public float Min = 1f;

        /// <summary>
        /// 
        /// </summary>
        public float Max = 1f;

        /// <summary>
        /// 
        /// </summary>
        public float Default = 1f;

        [NonSerialized] public EInstructionType OperationType;

        public bool isInverted => Max < Min;
    }

    public class ExtraArchiteData
    {
        /// <summary>
        /// 
        /// </summary>
        public float Fixed = 0f;

        /// <summary>
        /// 
        /// </summary>
        public float Factor = 1f;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            if (OperationAdd.Default < OperationAdd.Min || OperationAdd.Default > OperationAdd.Max)
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationAdd Default < Min || Default > Max";
            if (Mathf.Approximately(OperationAdd.Min, OperationAdd.Default) && Mathf.Approximately(OperationAdd.Max, OperationAdd.Default))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationAdd Default==Min==Max";
        }

        if (OperationMul is not null)
        {
            if (OperationMul.Default < OperationMul.Min || OperationMul.Default > OperationMul.Max)
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationMul Default < Min || Default > Max";
            if (Mathf.Approximately(OperationMul.Min, OperationMul.Default) && Mathf.Approximately(OperationMul.Max, OperationMul.Default))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationMul Default==Min==Max";
        }

        if (OperationSet is not null)
        {
            if (OperationSet.Default < OperationSet.Min || OperationSet.Default > OperationSet.Max)
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationSet Default < Min || Default > Max";
            if (Mathf.Approximately(OperationSet.Min, OperationSet.Default) && Mathf.Approximately(OperationSet.Max, OperationSet.Default))
                yield return $"JAI: StatDefinitionDef[defName={defName}]/OperationSet Default==Min==Max";
        }
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
}
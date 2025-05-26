using System.Diagnostics.CodeAnalysis;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class InstructionStat(StatDefinitionDef definition, StatDefinitionDef.Operation operation) : AInstruction(definition, operation)
{
    /// <summary>
    ///     IExposable constructor
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private InstructionStat() : this(default, null)
    {
    }

    private StatDef StatDef => Definition.StatDef;

    public override string Label => StatDef.LabelCap;

    public override void RenderExtraLine(StringBuilder sb)
    {
        if (Definition.Target.HasFlag(EInstructionTarget.Apparel))
            sb.Append("Apparel ");
        if (Definition.Target.HasFlag(EInstructionTarget.MeleeWeapon))
            sb.Append("Melee ");
        if (Definition.Target.HasFlag(EInstructionTarget.RangedWeapon))
            sb.Append("Ranged ");
    }

    public bool IsValidFor(Thing requester)
    {
        return requester is Pawn == Definition.IsPawnStat;
    }

    public virtual void TransformStatValue(StatDef statDef, ref float value, Thing requester)
    {
        if (statDef != StatDef) return;
        if (!IsValidFor(requester)) return;

        switch (OperationType)
        {
            case EInstructionType.Add:
                value += Value;
                break;
            case EInstructionType.Mul:
                value *= Value;
                break;
            case EInstructionType.Force:
                value = Value;
                break;
        }
    }

    public virtual bool AddStatExplanation(StatDef statDef, StringBuilder sb, Thing requester)
    {
        if (statDef != StatDef) return false;
        if (!IsValidFor(requester)) return false;

        sb.Append("\t").Append(Label);
        switch (OperationType)
        {
            case EInstructionType.Add:
                sb.Append(Value > 0 ? ": +" : ": -").Append(statDef.ValueToString(Mathf.Abs(Value))).AppendLine();
                break;
            case EInstructionType.Mul:
                sb.Append(": x").Append(Value).AppendLine();
                break;
            case EInstructionType.Force:
                sb.Append(Value > 0 ? ": =" : ": = -").Append(statDef.ValueToString(Mathf.Abs(Value))).AppendLine();
                break;
        }

        return true;
    }
}
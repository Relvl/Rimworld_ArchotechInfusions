using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public abstract class AInstruction : IExposable
{
    public float Complexity;
    public EInstructionType Type = EInstructionType.Add;
    public EInstructionTarget TypeFilter = EInstructionTarget.None;
    public float Value;

    public virtual string Label => "";
    public virtual Color BgColor => Color.white;

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref Type, "Type");
        Scribe_Values.Look(ref Value, "Value");
        Scribe_Values.Look(ref Complexity, "Complexity");
        Scribe_Values.Look(ref TypeFilter, "TypeFilter");
    }

    public virtual void RenderLabel(StringBuilder sb)
    {
        if (Complexity > 0)
            sb.Append("<color=#00FF00>").Append(Label).Append("</color> ");
        else
            sb.Append(Label).Append(" ");
    }

    public virtual void RenderValue(StringBuilder sb)
    {
        switch (Type)
        {
            case EInstructionType.Add:
                sb.Append("<color=#E57AFF>").Append(Value.ToString("+0.##;-0.##")).Append("</color> ");
                break;
            case EInstructionType.Mul:
                sb.Append("<color=#00FFFF>x").Append(Value.ToString("0.##")).Append("</color> ");
                break;
            case EInstructionType.Force:
                sb.Append("<color=#FF5555>=").Append(Value.ToString("0.##")).Append("</color> ");
                break;
        }
    }

    public virtual void RenderComplexity(StringBuilder sb)
    {
        if (Complexity > 0)
            sb.Append("Complexity: ").Append(Complexity.ToString("0.##"));
        else
            sb.Append("<color=#00FF00>Complexity: ").Append(Complexity.ToString("0.##")).Append("</color>");
    }

    public virtual void RenderExtraLine(StringBuilder sb)
    {
    }

    public virtual void RenderTooltip(StringBuilder sb)
    {
    }

    public virtual bool IsThingApplicable(Thing thing)
    {
        if (!thing.def.useHitPoints) return false;
        if (thing.def.IsWeapon && (thing.def.IsDrug || thing.def.IsIngestible)) return false;
        if (TypeFilter.HasFlag(EInstructionTarget.Apparel) && thing is Apparel) return true;
        if (TypeFilter.HasFlag(EInstructionTarget.MeleeWeapon) && thing.def.IsMeleeWeapon) return true;
        if (TypeFilter.HasFlag(EInstructionTarget.RangedWeapon) && thing.def.IsRangedWeapon) return true;
        return false;
    }
}
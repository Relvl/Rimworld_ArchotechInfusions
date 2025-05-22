using System.Text;
using ArchotechInfusions.injected;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.instructions;

public abstract class AInstruction(StatDefinitionDef definition, StatDefinitionDef.Operation operation) : IExposable
{
    private float? _complexityCached;
    private EInstructionType _operationTypeCached = operation?.OperationType ?? EInstructionType.Add;

    protected StatDefinitionDef Definition = definition;

    public float Value;
    public EInstructionType Type => _operationTypeCached;
    public StatDefinitionDef.Operation Operation { get; private set; } = operation;

    public virtual string Label => "";
    public virtual Color BgColor => Color.white;

    public float Complexity
    {
        get
        {
            _complexityCached ??= GetComplexity();
            return (float)_complexityCached;
        }
    }

    public virtual void ExposeData()
    {
        Scribe_Defs.Look(ref Definition, nameof(Definition));
        Scribe_Values.Look(ref _operationTypeCached, nameof(Type));
        Scribe_Values.Look(ref Value, nameof(Value));

        Operation = Definition.GetOperation(_operationTypeCached);
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

    protected virtual float GetComplexity()
    {
        // don't make the insane math anymore please... it makes me sick. old good conditions is more clear.
        float factor;
        var operation = Definition.GetOperation(Type);
        // lesser is better and uses more complexity
        if (operation.IsInverted)
        {
            // is "positive" effect and more complexity
            if (Value <= operation.Default)
                factor = (Value - operation.Default) / (operation.Max - operation.Default);
            // is "negative" effect and lowers the complexity
            else
                factor = (Value - operation.Default) / (operation.Default - operation.Min);
        }
        // greater is better and uses more complexity
        else
        {
            // is "positive" effect and more complexity
            if (Value >= operation.Default)
                factor = (Value - operation.Default) / (operation.Max - operation.Default);
            // is "negative" effect and lowers the complexity
            else
                factor = (Value - operation.Default) / (operation.Default - operation.Min);
        }

        return Mathf.Lerp(-Definition.Complexity / /*todo config? def? */10f, Definition.Complexity, (factor + 1f) / 2f);
    }

    public virtual bool IsThingApplicable(Thing thing, Comp_ArchInfused comp)
    {
        if (!thing.def.useHitPoints) return false;
        if (thing.def.IsWeapon && (thing.def.IsDrug || thing.def.IsIngestible)) return false;
        if (Definition.Target.HasFlag(EInstructionTarget.Apparel) && thing is Apparel) return true;
        if (Definition.Target.HasFlag(EInstructionTarget.MeleeWeapon) && thing.def.IsMeleeWeapon) return true;
        if (Definition.Target.HasFlag(EInstructionTarget.RangedWeapon) && thing.def.IsRangedWeapon) return true;
        return false;
    }
}
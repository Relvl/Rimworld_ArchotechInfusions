using System.Text;
using Verse;

namespace ArchotechInfusions.instructions;

public abstract class AInstruction : IExposable
{
    public float Complexity;
    public EInstructionType Type = EInstructionType.Add;
    public EInstructionTarget TypeFilter = EInstructionTarget.None;
    public float Value;

    public virtual string Label => "";


    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref Type, "Type");
        Scribe_Values.Look(ref Value, "Value");
        Scribe_Values.Look(ref Complexity, "Complexity");
        Scribe_Values.Look(ref TypeFilter, "TypeFilter");
    }

    public virtual void FillValueString(StringBuilder sb)
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
}
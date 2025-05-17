using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.statcollectors;

public class Instruction : IExposable
{
    public float Complexity;
    public StatDef Def;
    public TechLevel TechLevel;
    public InstructionType Type = InstructionType.Add;
    public InstructionTarget TypeFilter = InstructionTarget.None;
    public float Value;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref Def, "stat");
        Scribe_Values.Look(ref Type, "type");
        Scribe_Values.Look(ref Value, "value");
        Scribe_Values.Look(ref TechLevel, "techLevel");
        Scribe_Values.Look(ref Complexity, "complexity");
        Scribe_Values.Look(ref TypeFilter, "typeFilter");
    }

    public void FillValueString(StringBuilder sb)
    {
        switch (Type)
        {
            case InstructionType.Add:
                sb.Append("<color=#E57AFF>").Append(Value.ToString("+0.##;-0.##")).Append("</color> ");
                break;
            case InstructionType.Mul:
                sb.Append("<color=#00FFFF>x").Append(Value.ToString("0.##")).Append("</color> ");
                break;
            case InstructionType.Force:
                sb.Append("<color=#FF5555>=").Append(Value.ToString("0.##")).Append("</color> ");
                break;
        }
    }
}
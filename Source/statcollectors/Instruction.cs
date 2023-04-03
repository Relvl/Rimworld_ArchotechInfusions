using System;
using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.statcollectors;

public class Instruction : IExposable
{
    public StatDef Def;
    public InstructionType Type = InstructionType.Add;
    public float Value;
    public TechLevel TechLevel;
    public float Complexity;
    public InstructionTarget TypeFilter = InstructionTarget.None;

    public void ExposeData()
    {
        var defName = Def.defName;
        Scribe_Values.Look(ref defName, "statDefName");
        Scribe_Values.Look(ref Type, "type");
        Scribe_Values.Look(ref Value, "value");
        Scribe_Values.Look(ref TechLevel, "techLevel");
        Scribe_Values.Look(ref Complexity, "complexity");
        Scribe_Values.Look(ref TypeFilter, "typeFilter");
        if (Scribe.mode == LoadSaveMode.LoadingVars) Def = StatDef.Named(defName);
    }

    public void FillValueString(StringBuilder sb)
    {
        switch (Type)
        {
            case InstructionType.Add:
                sb.Append("<color=#4D00FF>+").Append(Value.ToString("0.##")).Append("</color> ");
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
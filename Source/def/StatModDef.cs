using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.def;

// ReSharper disable ClassNeverInstantiated.Global,FieldCanBeMadeReadOnly.Local,FieldCanBeMadeReadOnly.Global,ConvertToConstant.Global
// ReSharper disable InconsistentNaming,MemberCanBePrivate.Global
public class StatModDef : Def
{
    public TechLevel TechLevel = TechLevel.Neolithic;

    public InstructionTarget TypeFilter = InstructionTarget.None;

    public bool IsNegated = false;
    public float Complexity = 100;

    public Vector3 Add = Vector3.zero;
    public Vector3 Mul = Vector3.zero;
    public Vector3 Force = Vector3.zero;

    public void Generate(out float value, out InstructionType type, out float complexity)
    {
        var pairs = new[] { (Add, InstructionType.Add), (Mul, InstructionType.Mul), (Force, InstructionType.Force) };
        var (range, instructionType) = pairs.RandomElementByWeight(p => p.Item1.z);
        value = Rand.Range(range.x, range.y);
        type = instructionType;
        complexity = (value - range.x) / (range.y - range.x);
        if (IsNegated) complexity = 1 - complexity;
        complexity *= Mathf.Pow((float)TechLevel, 1.2f);
        complexity *= Complexity;
    }
}
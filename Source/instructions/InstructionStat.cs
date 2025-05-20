using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.instructions;

public class InstructionStat : AInstruction
{
    public StatDef Def;

    public override string Label => Def.LabelCap;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref Def, "stat");
    }

    public override void RenderExtraLine(StringBuilder sb)
    {
        if (TypeFilter.HasFlag(EInstructionTarget.Apparel))
            sb.Append("Apparel ");
        if (TypeFilter.HasFlag(EInstructionTarget.MeleeWeapon))
            sb.Append("Melee ");
        if (TypeFilter.HasFlag(EInstructionTarget.RangedWeapon))
            sb.Append("Ranged ");
    }
}
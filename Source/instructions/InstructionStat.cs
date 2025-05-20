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
}
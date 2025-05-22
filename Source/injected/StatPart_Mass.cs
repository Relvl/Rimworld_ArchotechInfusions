using RimWorld;

namespace ArchotechInfusions.injected;

public class StatPart_Mass : StatPart
{
    public StatPart_Mass(StatDef parentStat)
    {
        this.parentStat = parentStat;
    }
    
    public override void TransformValue(StatRequest req, ref float val)
    {
    }

    public override string ExplanationPart(StatRequest req)
    {
        return null;
    }
}
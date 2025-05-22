using RimWorld;

namespace ArchotechInfusions.injected;

public class StatPart_MaxHitPoints : StatPart
{
    public StatPart_MaxHitPoints(StatDef parentStat)
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
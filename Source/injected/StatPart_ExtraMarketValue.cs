using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace ArchotechInfusions.injected;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class StatPart_ExtraMarketValue : StatPart
{
    public StatPart_ExtraMarketValue(StatDef parentStat)
    {
        this.parentStat = parentStat;
    }


    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.HasThing) return;
        
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!req.HasThing) return null;

        return null;
    }
}
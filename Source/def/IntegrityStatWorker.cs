using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.injected;
using RimWorld;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class IntegrityStatWorker : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && req.Thing.HasComp<Comp_ArchInfused>();
    }

    public override bool IsDisabledFor(Thing thing)
    {
        return thing is Pawn || (!thing.def.IsApparel && !thing.def.IsWeapon);
    }

    public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
    {
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        if (!req.HasThing || !req.Thing.TryGetInfusedComp(out var comp)) return 0;
        return comp.Integrity;
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        if (!req.HasThing || !req.Thing.TryGetInfusedComp(out var comp)) return null;
        return "JAI.Stat.Integrity.InitialValue".Translate(comp.InitialIntegrity);
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        return null;
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest statRequest)
    {
        if (!statRequest.HasThing) yield break;
        yield return new Dialog_InfoCard.Hyperlink(ArchInf_ExternalUrls.ArchInf_ExternalUrl_Integrity_Wiki);
    }
}
using UnityEngine;
using Verse;

namespace ArchotechInfusions;

public class ArchotechInfusionsSettings : ModSettings
{
    public bool HideLoomBelowFlooring = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref HideLoomBelowFlooring, nameof(HideLoomBelowFlooring));
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var list = new Listing_Standard(GameFont.Small);
        list.Begin(inRect);
        list.CheckboxLabeled("ArchotechInfusions.Settings.HideLoomBelowFlooring".Translate(), ref HideLoomBelowFlooring);
        list.End();
    }
}
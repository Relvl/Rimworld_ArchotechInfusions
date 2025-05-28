using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ArchotechInfusions.injected;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ArchotechInfusionsMod : Mod
{
    public static readonly Color ButtonWarningColor = new(1f, 0.3f, 0.35f);

    public ArchotechInfusionsMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<ArchotechInfusionsSettings>();

        new Harmony("johnson1893.archotech.infusions")
            .PatchAll(Assembly.GetExecutingAssembly());

        LongEventHandler.ExecuteWhenFinished(InjectDefParts);
    }

    public static ArchotechInfusionsSettings Settings { get; private set; } = new();

    private static void InjectDefParts()
    {
        var compProps = new CompProperties { compClass = typeof(InstructionsComps) };
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(InjectThingPredicate))
            thingDef.comps.Add(compProps);

        StatDefOf.MarketValue.parts ??= [];
        StatDefOf.MarketValue.parts.Add(new StatPart_ExtraMarketValue(StatDefOf.MarketValue));
    }

    private static bool InjectThingPredicate(ThingDef def)
    {
        if (!def.HasComp<CompQuality>()) return false;
        if (def.HasComp<InstructionsComps>()) return false;

        if (def.IsApparel) return true;
        if (def.IsRangedWeapon) return true;
        if (def.IsMeleeWeapon && !def.IsDrug && !def.IsIngestible) return true;

        return false;
    }

    public override string SettingsCategory()
    {
        return "Archotech Infusions";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ArchotechInfusions.graphic;
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
    public static readonly Material AccumulatorBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.85f, 0.85f));
    public static readonly Material AccumulatorBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));
    public static readonly Material ContainerFuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));
    public static readonly Material ContainerFuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    public static readonly GraphicGridOverlay Overlay = new(
        GraphicDatabase.Get<Graphic_Single>( //
            "ArchotechInfusions/Things/GridOverlay_Atlas",
            ShaderDatabase.MetaOverlay,
            Vector2.one,
            new Color32(159, 217, 60, 190)
        )
    );

    public ArchotechInfusionsMod(ModContentPack content) : base(content)
    {
        new Harmony("johnson1893.archotech.infusions")
            .PatchAll(Assembly.GetExecutingAssembly());

        LongEventHandler.ExecuteWhenFinished(InjectDefParts);
    }

    private static void InjectDefParts()
    {
        var compProps = new CompProperties { compClass = typeof(Comp_ArchInfused) };
        var iTabResolved = InspectTabManager.GetSharedInstance(typeof(ITab_ArchotechInfusion));
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(InjectThingPredicate))
        {
            thingDef.comps.Add(compProps);
            thingDef.inspectorTabs ??= [];
            thingDef.inspectorTabs.Add(typeof(ITab_ArchotechInfusion));
            thingDef.inspectorTabsResolved ??= [];
            thingDef.inspectorTabsResolved.Add(iTabResolved);
        }

        StatDefOf.MarketValue.parts ??= [];
        StatDefOf.MarketValue.parts.Add(new StatPart_ExtraMarketValue(StatDefOf.MarketValue));
    }

    private static bool InjectThingPredicate(ThingDef def)
    {
        if (!def.HasComp<CompQuality>()) return false;
        if (def.HasComp<Comp_ArchInfused>()) return false;

        if (def.IsApparel) return true;
        if (def.IsRangedWeapon) return true;
        if (def.IsMeleeWeapon && !def.IsDrug && !def.IsIngestible) return true;

        return false;
    }
}
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ArchInf_Repairer_Building : AGridBuilding
{
    private float _architeCache;

    private float _chargeCache;
    private RepairerGizmo _gizmo;
    private float _progressCache;

    /// <summary>
    /// </summary>
    private QualityRange _qualityRange = new(QualityCategory.Good, QualityCategory.Legendary);

    private Comp_ArchiteRepairer _repairerComp;

    /// <summary>
    ///     Item's HP level from which pawn will repair it.
    ///     Item's HP level to which pawn will repair it.
    /// </summary>
    private FloatRange _repairLevel = new(0.8f, 1f);

    /// <summary>
    ///     Ticks to new repair cycle
    /// </summary>
    private int _ticksCurrentCycle;

    public Comp_ArchiteRepairer RepairerComp => _repairerComp ??= GetComp<Comp_ArchiteRepairer>();


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _repairLevel, "RepairLevel");
        Scribe_Values.Look(ref _ticksCurrentCycle, "TicksCurrentCycle");
        Scribe_Values.Look(ref _qualityRange, "QualityRange");
    }

    public override void Tick()
    {
        base.Tick();
        _chargeCache = Grid.GetTotalEnergy();
        _architeCache = Grid.GetTotalArchite();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action
            {
                defaultLabel = "Damage all apparel", action = () =>
                {
                    foreach (var pawn in RepairerComp.parent.Map.mapPawns.AllPawnsSpawned.Where(c => c.IsColonist))
                    foreach (var apparel in pawn.apparel.WornApparel.Where(apparel => apparel.def.useHitPoints))
                        apparel.HitPoints = apparel.MaxHitPoints / 2;

                    Messages.Message("All apparel damaged", this, MessageTypeDefOf.CautionInput, false);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Log stats", action = () =>
                {
                    Log.Warning($"{LabelCap}: can work: {CanWork()}");
                    Log.Warning($"{LabelCap}: charge cache: {_chargeCache}");
                    Log.Warning($"{LabelCap}: archite cache: {_architeCache}");
                    Log.Warning($"{LabelCap}: progress cache: {_progressCache}");
                    Log.Warning($"{LabelCap}: Props.HpPerTick: {RepairerComp.Props.HpPerTick}");
                    Log.Warning($"{LabelCap}: Props.ArchitePerHp: {RepairerComp.Props.ArchitePerHp}");
                    Log.Warning($"{LabelCap}: Props.EnergyPerHp: {RepairerComp.Props.EnergyPerHp}");
                    foreach (var pawn in Map.mapPawns.AllPawnsSpawned.Where(p => p.IsColonist))
                    {
                        var thing = GetAllRepairableThings(pawn).MinBy(t => (float)t.HitPoints / t.MaxHitPoints);
                        Log.Warning($"Pawn: {pawn.LabelCap}: {thing?.LabelCap} - {thing?.HitPoints}/{thing?.MaxHitPoints}");
                    }
                }
            };
        }

        yield return _gizmo ??= new RepairerGizmo(this);
    }

    public bool CanWork()
    {
        if (RepairerComp.Props.HpPerTick == 0) return false;
        if (!Grid.PowerOn) return false;
        if (_chargeCache < RepairerComp.Props.EnergyPerHp * 10) return false;
        if (_architeCache < RepairerComp.Props.ArchitePerHp * 10) return false;
        if (!Grid.Get<ArchInf_Container_Building>().Any(c => c.Stored >= RepairerComp.Props.ArchitePerHp)) return false;
        return true;
    }

    public float GetPercentComplete()
    {
        return _progressCache;
    }

    public IEnumerable<Thing> GetAllRepairableThings(Pawn pawn)
    {
        var isCurrentlyRepairing = pawn.CurJob?.def == JobDriverDefOf.ArchInf_RepairInventory;
        foreach (var apparel in pawn.apparel.WornApparel)
        {
            if (!apparel.def.useHitPoints) continue;

            if (apparel.TryGetQuality(out var qc))
            {
                if (qc < _qualityRange.min) continue;
                if (qc > _qualityRange.max) continue;
            }

            if (apparel.HitPoints >= apparel.MaxHitPoints) continue;
            var hpPercent = (float)apparel.HitPoints / apparel.MaxHitPoints;

            if (isCurrentlyRepairing)
            {
                // if the pawn already work on it - use RepairLevelMax
                if (hpPercent > _repairLevel.max) continue;
            }
            else
            {
                // if this is new item for repair - use RepairLevelMin, we don't want to repair too often
                if (hpPercent > _repairLevel.min) continue;
            }

            yield return apparel;
        }
    }

    public void DoJobTick(Pawn pawn, JobDriver driver, float speed)
    {
        if (!CanWork())
        {
            driver.EndJobWith(JobCondition.Incompletable);
            _ticksCurrentCycle = 0;
            return;
        }

        GetAllRepairableThings(pawn).ToList().TryMinBy(t => (float)t.HitPoints / t.MaxHitPoints, out var thing);
        if (thing == null)
        {
            driver.EndJobWith(JobCondition.Succeeded);
            _ticksCurrentCycle = 0;
            return;
        }

        _progressCache = (float)thing.HitPoints / thing.MaxHitPoints;
        var hpPerTick = RepairerComp.Props.HpPerTick * speed;
        var ticksToRestore = 1 / hpPerTick;

        if (_ticksCurrentCycle >= ticksToRestore)
        {
            _ticksCurrentCycle = 0;
            Restore(thing, pawn, driver, 1);
        }

        _ticksCurrentCycle++;
    }

    private void Restore(Thing thing, Pawn pawn, JobDriver driver, int hp)
    {
        var energy = RepairerComp.Props.EnergyPerHp * hp;
        var archite = RepairerComp.Props.ArchitePerHp * hp;
        if (energy > _chargeCache || archite > _architeCache)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            _ticksCurrentCycle = 0;
            return;
        }

        Grid.ConsumeEnergy(ref energy);
        // still want after consume
        if (energy > 0)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            _ticksCurrentCycle = 0;
            return;
        }

        Grid.ConsumeArchite(ref archite);
        // still want after consume
        if (archite > 0)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            return;
        }


        pawn.skills.Learn(SkillDefOf.Crafting, 0.0001f);
        pawn.GainComfortFromCellIfPossible(true);
        thing.HitPoints += hp;
    }

    private class RepairerGizmo(ArchInf_Repairer_Building repairer) : Gizmo
    {
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var innerRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            var contentRect = innerRect.ContractedBy(6f);
            var sliderHeight = contentRect.height / 2.5f + 2f;

            if (Mouse.IsOver(innerRect)) TooltipHandler.TipRegion(innerRect, "JAI.Gizmo.Repairer.Title.Desc".Translate());

            Widgets.DrawWindowBackground(innerRect);
            GUI.BeginGroup(contentRect);

            var hpRect = new Rect(0f, 0f, contentRect.width, sliderHeight);
            Widgets.FloatRange(hpRect, 1, ref repairer._repairLevel, valueStyle: ToStringStyle.PercentZero);

            var qualityRect = new Rect(0f, hpRect.yMax + 2f, contentRect.width, sliderHeight);
            Widgets.QualityRange(qualityRect, 876813230, ref repairer._qualityRange);

            GUI.EndGroup();
            return new GizmoResult(GizmoState.Clear);
        }

        public override float GetWidth(float maxWidth)
        {
            return 180f;
        }
    }
}
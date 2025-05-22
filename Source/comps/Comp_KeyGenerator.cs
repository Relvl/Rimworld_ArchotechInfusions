using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.comps.comp_base;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class CompProps_KeyGenerator : CompProperties
{
    public int AccumulatorRecacheTicks = 60;
    public int MaxStoredKeys = 3;
    public int TotalEnergyCost = 200;
    public int WorkAmount = 1500;

    public CompProps_KeyGenerator()
    {
        compClass = typeof(Comp_KeyGenerator);
    }
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Comp_KeyGenerator : CompBase_Grid<CompProps_KeyGenerator>
{
    private float _chargeCache;
    private int _keysStored;

    public float Progress;
    public float EstimateEnergyPerTick => (float)Props.TotalEnergyCost / Props.WorkAmount;

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref Progress, "progress");
        Scribe_Values.Look(ref _keysStored, "keysStored");
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (signal == "Breakdown")
            Progress = 0;
    }

    public override void CompTick()
    {
        _chargeCache = Member.Grid.GetTotalEnergy();
    }

    public bool IsPowerEnough()
    {
        return _chargeCache > EstimateEnergyPerTick * Props.AccumulatorRecacheTicks;
    }

    public bool CanGenerateNewKey()
    {
        return Power.PowerOn
               && _keysStored < Props.MaxStoredKeys
               && IsPowerEnough();
    }

    public void DoJobTick(Pawn pawn, JobDriver driver, float speed)
    {
        if (!Power.PowerOn)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            return;
        }

        var wantedEnergy = (float)Props.TotalEnergyCost / Props.WorkAmount * speed;

        Member.Grid.ConsumeEnergy(ref wantedEnergy);

        // Just dirty cache fix, it will be actualized in the next tick.
        _chargeCache -= wantedEnergy;

        // still want after consume
        if (wantedEnergy > 0)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            return;
        }

        Progress = Math.Min(Props.WorkAmount, Progress + speed);
        pawn.skills.Learn(SkillDefOf.Intellectual, 0.01f);
        pawn.GainComfortFromCellIfPossible(true);

        if (Progress >= Props.WorkAmount)
        {
            _keysStored++;
            Progress = 0;
            driver.EndJobWith(JobCondition.Succeeded);
            // todo floating message over keygen
        }
    }

    public float GetPercentComplete()
    {
        if (!CanGenerateNewKey()) return 0;
        return Progress / Props.WorkAmount;
    }

    public bool TryConsumeKey()
    {
        if (_keysStored <= 0) return false;
        _keysStored--;
        return true;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();

        if (!Power.PowerOn)
            sb.AppendLine("JAI.Error.PoweredOff".Translate());
        if (!IsPowerEnough())
            sb.AppendLine("JAI.Error.BatteriesUncharged".Translate());

        if (Progress > 0)
            sb.AppendLine("JAI.Progress".Translate((GetPercentComplete() * 100f).ToString("0.")));

        sb.AppendLine("JAI.KeyGenerator.KeysCount".Translate(_keysStored));

        return sb.TrimEnd().ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (!DebugSettings.ShowDevGizmos) yield break;
        yield return new Command_Action { Disabled = _keysStored == 0, defaultLabel = "-1 key", action = () => _keysStored-- };
        yield return new Command_Action { defaultLabel = "+1 key", action = () => _keysStored++ };
    }
}
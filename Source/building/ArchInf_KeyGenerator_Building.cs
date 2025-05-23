using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.building;

public class ArchInf_KeyGenerator_Building : AddInf_Building
{
    private float _chargeCache;
    private Comp_KeyGenerator _comp;
    private int _keysStored;

    public float Progress;
    public Comp_KeyGenerator Comp => _comp ??= GetComp<Comp_KeyGenerator>();
    public float EstimateEnergyPerTick => (float)Comp.Props.TotalEnergyCost / Comp.Props.WorkAmount;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref Progress, "progress");
        Scribe_Values.Look(ref _keysStored, "keysStored");
    }

    public override void Tick()
    {
        base.Tick();
        _chargeCache = Grid.GetTotalEnergy();
    }

    public override string GetInspectString()
    {
        return base.GetInspectString() + "\n" + CompInspectStringExtra();
    }

    protected override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);
        if (signal == "Breakdown")
            Progress = 0;
    }

    public string CompInspectStringExtra()
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

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { Disabled = _keysStored == 0, defaultLabel = "-1 key", action = () => _keysStored-- };
            yield return new Command_Action { defaultLabel = "+1 key", action = () => _keysStored++ };
        }
    }


    public bool IsPowerEnough()
    {
        return _chargeCache > EstimateEnergyPerTick * Comp.Props.AccumulatorRecacheTicks;
    }

    public bool CanGenerateNewKey()
    {
        return Power.PowerOn
               && _keysStored < Comp.Props.MaxStoredKeys
               && IsPowerEnough();
    }

    public void DoJobTick(Pawn pawn, JobDriver driver, float speed)
    {
        if (!Power.PowerOn)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            return;
        }

        var wantedEnergy = (float)Comp.Props.TotalEnergyCost / Comp.Props.WorkAmount * speed;

        Grid.ConsumeEnergy(ref wantedEnergy);

        // Just dirty cache fix, it will be actualized in the next tick.
        _chargeCache -= wantedEnergy;

        // still want after consume
        if (wantedEnergy > 0)
        {
            driver.EndJobWith(JobCondition.Incompletable);
            return;
        }

        Progress = Math.Min(Comp.Props.WorkAmount, Progress + speed);
        pawn.skills.Learn(SkillDefOf.Intellectual, 0.01f);
        pawn.GainComfortFromCellIfPossible(true);

        if (Progress >= Comp.Props.WorkAmount)
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
        return Progress / Comp.Props.WorkAmount;
    }

    public bool TryConsumeKey()
    {
        if (_keysStored <= 0) return false;
        _keysStored--;
        return true;
    }
}
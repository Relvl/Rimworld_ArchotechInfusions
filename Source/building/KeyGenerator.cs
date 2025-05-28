using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class KeyGenerator : AGridBuilding
{
    private float _chargeCache;
    private KeyGeneratorProps.Comp _comp;
    private int _keysStored;

    public float Progress;
    public KeyGeneratorProps.Comp Comp => _comp ??= GetComp<KeyGeneratorProps.Comp>();
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

    protected override void FillInspectStringExtra(StringBuilder sb)
    {
        if (!Grid.PowerOn)
            sb.AppendLine("JAI.Error.PoweredOff".Translate());
        if (!IsPowerEnough())
            sb.AppendLine("JAI.Error.BatteriesUncharged".Translate());

        if (Progress > 0)
            sb.AppendLine("JAI.Progress".Translate((GetPercentComplete() * 100f).ToString("0.")));

        sb.AppendLine("JAI.KeyGenerator.KeysCount".Translate(_keysStored));
    }

    protected override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);
        if (signal == "Breakdown")
            Progress = 0;
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
        return Grid.PowerOn
               && _keysStored < Comp.Props.MaxStoredKeys
               && IsPowerEnough();
    }

    public void DoJobTick(Pawn pawn, JobDriver driver, float speed)
    {
        if (!Grid.PowerOn)
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
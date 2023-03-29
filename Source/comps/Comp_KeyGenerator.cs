using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_KeyGenerator : CompProperties
{
    public int WorkAmmount = 1500;
    public int MaxStoredKeys = 3;
    public int TotalEnergyCost = 200;
    public int AccumulatorRecacheTicks = 60;
    public CompProps_KeyGenerator() => compClass = typeof(Comp_KeyGenerator);
}

public class Comp_KeyGenerator : CompBase_Membered<CompProps_KeyGenerator>
{
    private float _accumulatorsCharge;
    private float EstimateEnergyPerTick => (float)Props.TotalEnergyCost / Props.WorkAmmount;

    public readonly List<Guid> Keys = new();
    public float Progress;

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref Progress, "progress");

        var keyCount = Keys.Count;
        Scribe_Values.Look(ref keyCount, "keysStored");
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            Keys.Clear();
            for (var i = 0; i < keyCount; i++) Keys.Add(Guid.NewGuid());
        }
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (signal == "Breakdown")
        {
            Progress = 0;
        }
    }

    public override void CompTick()
    {
        if (parent.IsHashIntervalTick(Props.AccumulatorRecacheTicks) || _accumulatorsCharge < EstimateEnergyPerTick * Props.AccumulatorRecacheTicks * 2)
        {
            _accumulatorsCharge = Member.Grid.GetComps<Comp_Accumulator>().Sum(a => a.Stored);
        }
    }

    public bool CanGenerateNewKey()
    {
        return Power.PowerOn && Keys.Count < Props.MaxStoredKeys && _accumulatorsCharge > EstimateEnergyPerTick * Props.AccumulatorRecacheTicks;
    }

    public void DoGenerateTick(Pawn pawn, JobDriver driver, float speed)
    {
        Log.Message($"DoGenerateTick with speed {speed}");
        var energy = (float)Props.TotalEnergyCost / Props.WorkAmmount * speed;

        _accumulatorsCharge -= energy;

        foreach (var accumulator in Member.Grid.GetComps<Comp_Accumulator>())
        {
            accumulator.Consume(ref energy);
            if (energy <= 0) break;
        }

        if (energy > 0)
        {
            Log.Warning($"Cannot drain energy < {energy}");
            driver.EndJobWith(JobCondition.Incompletable);
            return;
        }

        Progress = Math.Min(Props.WorkAmmount, Progress + speed);
        pawn.skills.Learn(SkillDefOf.Intellectual, 0.01f);
        pawn.GainComfortFromCellIfPossible(true);

        if (Progress >= Props.WorkAmmount)
        {
            Keys.Add(Guid.NewGuid());
            Progress = 0;
            driver.EndJobWith(JobCondition.Succeeded);

            Messages.Message("DEV: new key generated!", parent, MessageTypeDefOf.RejectInput);
        }
    }

    public float GetPercentComplete()
    {
        if (!CanGenerateNewKey()) return 0;
        return Progress / Props.WorkAmmount;
    }

    public bool HasFreeKey() => Keys.Count > 0;

    public Guid ConsumeKey()
    {
        if (Keys.Count == 0) return default;
        var key = Keys.First();
        Keys.Remove(key);
        return key;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();

        if (Progress > 0) sb.AppendLine($"Progress: {GetPercentComplete() * 100f:0.}%");
        sb.AppendLine($"Keys generated: {Keys.Count}");

        return sb.TrimEnd().ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { disabled = Keys.Count == 0, defaultLabel = "-1 key", action = () => Keys.Remove(Keys.First()) };
            yield return new Command_Action { defaultLabel = "+1 key", action = () => Keys.Add(Guid.NewGuid()) };
            yield return new Command_Action { defaultLabel = "Breakdown", action = () => parent.TryGetComp<CompBreakdownable>()?.DoBreakdown() };
        }
    }
}
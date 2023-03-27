using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps;

public class CompAccumulator : ThingComp
{
    private float _stored;
    private float _storedLastTick;
    private CompPowerTrader _power;

    public CompPropsAccumulator Props => props as CompPropsAccumulator;

    public float Stored
    {
        get => _stored;
        set => _stored = Math.Max(0, Math.Min(value, Props.MaxStored));
    }

    public CompPowerTrader CompPowerTrader => _power ??= parent.TryGetComp<CompPowerTrader>();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (CompPowerTrader is null) throw new Exception("ArchInf: CompAccumulator can't worl without CompPowerTrader");
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _stored, "stored");

        Stored = _stored;
        _storedLastTick = _stored;
    }

    public bool IsFull => Stored >= Props.MaxStored;

    public override void CompTick()
    {
        _storedLastTick = Stored;

        if (CompPowerTrader.PowerOn)
        {
            if (IsFull)
            {
                CompPowerTrader.PowerOutput = -CompPowerTrader.Props.idlePowerDraw;
            }
            else
            {
                CompPowerTrader.PowerOutput = -CompPowerTrader.Props.PowerConsumption;
                Stored -= CompPowerTrader.PowerOutput * CompPower.WattsToWattDaysPerTick * 10;
            }
        }

        base.CompTick();
    }

    public bool Consume(ref float energy)
    {
        // todo! balance tha accums
        if (Stored >= energy)
        {
            Stored -= energy;
            energy = 0;
            return true;
        }

        energy -= Math.Min(Stored, energy);
        Stored = 0;
        return false;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        if (DebugSettings.ShowDevGizmos) sb.AppendLine($"Diff: {(Stored - _storedLastTick) * CompPower.WattsToWattDaysPerTick * 10:+0.;0;-0.}");
        sb.Append($"Stored: {Stored:0.} / {Props.MaxStored:0.}");
        return sb.ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "DEV: Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "DEV: -10%", action = () => Stored -= Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: +10%", action = () => Stored += Props.MaxStored / 10 };
            yield return new Command_Action { defaultLabel = "DEV: Full", action = () => Stored = Props.MaxStored };
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui.print;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class CompProps_Printer : CompProperties
{
    public int PrintArchiteCost = 250;
    public int PrintEnergyCost = 1000;
    public int PrintTicks = 1000;

    public CompProps_Printer()
    {
        compClass = typeof(Comp_Printer);
    }
}

public class Comp_Printer : CompBase_Grid<CompProps_Printer>
{
    private AInstruction _instruction;
    private Thing _targetThing;
    private int _ticksCurrentCycle;

    private PrintWindowSelector _windowSelector;
    public PrintWindowSelector WindowSelector => _windowSelector ??= new PrintWindowSelector(this);

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Deep.Look(ref _instruction, "PrintingInstruction", LookMode.Deep);
        Scribe_Values.Look(ref _ticksCurrentCycle, "TicksCurrentCycle");
    }

    public bool HasEnoughPower()
    {
        return Grid.GetTotalEnergy() >= Props.PrintEnergyCost;
    }

    public bool HasEnoughArchite()
    {
        return Grid.GetTotalArchite() >= Props.PrintArchiteCost;
    }

    public bool HasAnyInstruction()
    {
        return Grid.GetComps<Comp_Database>().Any(db => db.Instructions.Count > 0);
    }

    public bool CanWork()
    {
        return Power.PowerOn && HasEnoughPower() && HasEnoughArchite();
    }

    public void OpenSelectThingWindow(Pawn pawn)
    {
        WindowSelector.OpenThingSelector(pawn);
    }

    public void SetInstruction(AInstruction instruction, Thing target)
    {
        _instruction = instruction;
        _targetThing = target;
        Grid.TryRemoveInstruction(instruction);
    }

    public void DoJobStarted(JobDriver driver)
    {
        if (_instruction is null)
        {
            driver.EndJobWith(JobCondition.Succeeded);
            return;
        }

        _ticksCurrentCycle = 0;
    }

    public void DoJobTick(JobDriver driver, Pawn pawn)
    {
        if (_targetThing is null || _instruction is null)
        {
            driver.EndJobWith(JobCondition.Errored);
            return;
        }

        if (_ticksCurrentCycle >= Props.PrintTicks)
        {
            if (!_targetThing.TryGetInfusedComp(out var comp))
            {
                Log.ErrorOnce($"JAI: Thing {_targetThing} has no ArchInfused comp", _targetThing.def.defName.GetHashCode());
                driver.EndJobWith(JobCondition.Errored);
                return;
            }

            float energy = Props.PrintEnergyCost;
            Grid.ConsumeEnergy(ref energy);
            if (energy > 0)
            {
                Messages.Message("AI Printer: not enough energy", parent, MessageTypeDefOf.TaskCompletion, false);
                driver.EndJobWith(JobCondition.Errored);
                return;
            }

            float archite = Props.PrintArchiteCost;
            _instruction.ModifyArchiteConsumed(ref archite);
            Grid.ConsumeArchite(ref archite);
            if (archite > 0)
            {
                Messages.Message("JAI.Printer.Print.Error.NoArchite".Translate(), parent, MessageTypeDefOf.TaskCompletion, false);
                driver.EndJobWith(JobCondition.Errored);
                return;
            }

            var (damageAmount, breakChance) = comp.GetApplyDamageFor(_instruction);

            if (breakChance > 0f)
            {
                var random = comp.TakeBreakRandom();
                if (random <= breakChance)
                {
                    Find.LetterStack.ReceiveLetter(
                        "JAI.Printer.Print.ThingDestroyed".Translate(),
                        "JAI.Printer.Print.ThingDestroyed.Desc".Translate(pawn.NameShortColored, _targetThing.LabelShort, GenLabel.ThingLabel(_targetThing, 1, true, false)),
                        LetterDefOf.ThreatSmall, parent
                    );
                    _targetThing.Destroy();
                    driver.EndJobWith(JobCondition.Succeeded); // ;]
                    return;
                }
            }

            if (damageAmount > 0)
                _targetThing.TakeDamage(new DamageInfo(
                    DamageDefOf.Deterioration,
                    damageAmount,
                    spawnFilth: false,
                    checkForJobOverride: false
                ));

            comp.Apply(_instruction);
            _instruction = null;

            driver.EndJobWith(JobCondition.Succeeded);
            return;
        }

        _ticksCurrentCycle++;
    }

    public void DoJobFinished()
    {
        WindowSelector.ForceClose();
        _targetThing = null;
        if (Grid.TryPutInstruction(_instruction))
            _instruction = null;
    }

    public float GetPercentComplete()
    {
        return (float)_ticksCurrentCycle / Props.PrintTicks;
    }
}
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui.print;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.building;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ArchInf_Printer_Building : AddInf_Building
{
    private AInstruction _instruction;
    private Comp_Printer _printerComp;
    private Thing _targetThing;
    private int _ticksCurrentCycle;

    private PrintWindowSelector _windowSelector;
    public Comp_Printer PrinterComp => _printerComp ??= GetComp<Comp_Printer>();
    public PrintWindowSelector WindowSelector => _windowSelector ??= new PrintWindowSelector(this);


    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Deep.Look(ref _instruction, "PrintingInstruction", LookMode.Deep);
        Scribe_Values.Look(ref _ticksCurrentCycle, "TicksCurrentCycle");
    }


    public bool HasEnoughPower()
    {
        return Grid.GetTotalEnergy() >= PrinterComp.Props.PrintEnergyCost;
    }

    public bool HasEnoughArchite()
    {
        return Grid.GetTotalArchite() >= PrinterComp.Props.PrintArchiteCost;
    }

    public bool HasAnyInstruction()
    {
        return Grid.Get<ArchInf_Database_Building>().Any(db => db.Instructions.Count > 0);
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

        if (_ticksCurrentCycle >= PrinterComp.Props.PrintTicks)
        {
            if (!_targetThing.TryGetInfusedComp(out var comp))
            {
                Log.ErrorOnce($"JAI: Thing {_targetThing} has no ArchInfused comp", _targetThing.def.defName.GetHashCode());
                driver.EndJobWith(JobCondition.Errored);
                return;
            }

            float energy = PrinterComp.Props.PrintEnergyCost;
            Grid.ConsumeEnergy(ref energy);
            if (energy > 0)
            {
                Messages.Message("AI Printer: not enough energy", this, MessageTypeDefOf.TaskCompletion, false);
                driver.EndJobWith(JobCondition.Errored);
                return;
            }

            float archite = PrinterComp.Props.PrintArchiteCost;
            _instruction.ModifyArchiteConsumed(ref archite);
            Grid.ConsumeArchite(ref archite);
            if (archite > 0)
            {
                Messages.Message("JAI.Printer.Print.Error.NoArchite".Translate(), this, MessageTypeDefOf.TaskCompletion, false);
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
                        LetterDefOf.ThreatSmall, this
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
        return (float)_ticksCurrentCycle / PrinterComp.Props.PrintTicks;
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        foreach (var option in base.GetFloatMenuOptions(selPawn))
            yield return option;

        if (!HasEnoughPower())
        {
            yield return new FloatMenuOption("JAI.FloatMenu.Printer.NoPower".Translate(), null);
            yield break;
        }

        if (!HasEnoughArchite())
        {
            yield return new FloatMenuOption("JAI.FloatMenu.Printer.NoArchite".Translate(), null);
            yield break;
        }

        if (!HasAnyInstruction())
        {
            yield return new FloatMenuOption("JAI.FloatMenu.Printer.NoInstructions".Translate(), null);
            yield break;
        }

        yield return new FloatMenuOption("JAI.FloatMenu.Printer.Start".Translate(), () => OrderStartPrinting(selPawn));
    }

    public void OrderStartPrinting(Pawn pawn)
    {
        pawn.jobs.TryTakeOrderedJob(new Job(JobDriverDefOf.ArchInf_Print, this, 1500), JobTag.Misc);
    }
}
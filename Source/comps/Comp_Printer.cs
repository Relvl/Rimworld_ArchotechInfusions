using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui;
using ArchotechInfusions.ui.print;
using Verse;
using Verse.AI;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Printer : CompProperties
{
    public int PrintEnergyCost = 1000;
    public int PrintTicks = 1000;
    public int PrintArchiteCost = 250;

    public CompProps_Printer()
    {
        compClass = typeof(Comp_Printer);
    }
}

public class Comp_Printer : CompBase_Grid<CompProps_Printer>
{
    private AInstruction _instruction;
    private int _ticksCurrentCycle;
    private Thing _targetThing;

    private PrintWindowSelector _windowSelector;
    public PrintWindowSelector WindowSelector => _windowSelector ??= new PrintWindowSelector(this);

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Deep.Look(ref _instruction, "PrintingInstruction", LookMode.Deep);
        Scribe_Values.Look(ref _ticksCurrentCycle, "TicksCurrentCycle");
    }

    public bool CanWork()
    {
        return Power.PowerOn;
    }

    public bool IsArchiteEnough()
    {
        return true;
    }

    public bool IsAnyInstructionStored()
    {
        return true;
    }

    public void OpenSelectThingWindow(Pawn pawn, JobDriver driver)
    {
        WindowSelector.OpenThingSelector(pawn);
    }

    public void EnqueueInstruction(AInstruction instruction, Thing target)
    {
        _instruction = instruction;
        _targetThing = target;
        Member.Grid.TryRemoveInstruction(instruction);
    }

    public void DoJobStarted(Pawn pawn, JobDriver driver, bool initial)
    {
        if (_instruction is null)
        {
            driver.EndJobWith(JobCondition.Succeeded);
            return;
        }

        _ticksCurrentCycle = 0;
    }

    public void DoJobTick(Pawn pawn, JobDriver driver, float speed)
    {
        if (_ticksCurrentCycle >= Props.PrintTicks)
        {
            var applyInstruction = _instruction;
            _instruction = null;
            ApplyInstruction(pawn, driver, applyInstruction);
            DoJobStarted(pawn, driver, false);
            return;
        }

        _ticksCurrentCycle++;
    }

    public void DoJobFinished(Pawn pawn, JobDriver driver)
    {
        WindowSelector.ForceClose();
        _targetThing = null;
        if (Member.Grid.TryPutInstruction(_instruction))
            _instruction = null;
    }


    public float GetPercentComplete()
    {
        return (float)_ticksCurrentCycle / Props.PrintTicks;
    }

    public void ApplyInstruction(Pawn pawn, JobDriver driver, AInstruction instruction)
    {
        Log.Warning($"-- apply instruction: {instruction}");
    }
}
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
    private AInstruction _currentInstruction;
    private List<AInstruction> _instructions = [];
    private int _ticksCurrentCycle;
    private Thing _targetThing;

    private PrintWindowSelector _windowSelector;
    public PrintWindowSelector WindowSelector => _windowSelector ??= new PrintWindowSelector(this);

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Collections.Look(ref _instructions, "Instructions", LookMode.Deep);
        Scribe_Deep.Look(ref _currentInstruction, "CurrentInstruction");
        Scribe_Values.Look(ref _ticksCurrentCycle, "TicksCurrentCycle");

        _instructions ??= [];
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
        _instructions.Add(instruction);
        _targetThing = target;
        Member.Grid.TryRemoveInstruction(instruction);
    }

    public void DoJobStarted(Pawn pawn, JobDriver driver, bool initial)
    {
        Log.Warning("-- on job started");

        if (_currentInstruction != null)
            Log.Error("JAI: printing started with active instruction, something goes wrong!");

        if (_instructions.Count == 0)
        {
            driver.EndJobWith(JobCondition.Succeeded);
            return;
        }

        _currentInstruction = _instructions.First();
        _instructions.Remove(_currentInstruction);
        _ticksCurrentCycle = 0;
    }

    public void DoJobTick(Pawn pawn, JobDriver driver, float speed)
    {
        if (_ticksCurrentCycle >= Props.PrintTicks)
        {
            var applyInstruction = _currentInstruction;
            _currentInstruction = null;
            ApplyInstruction(pawn, driver, applyInstruction);
            DoJobStarted(pawn, driver, false);
            return;
        }

        _ticksCurrentCycle++;
    }

    public void DoJobFinished(Pawn pawn, JobDriver driver)
    {
        Log.Warning("-- on job finished");
        WindowSelector.ForceClose();
        _targetThing = null;
        if (Member.Grid.TryPutInstruction(_currentInstruction))
            _currentInstruction = null;
        foreach (var instruction in _instructions.ToList())
            if (Member.Grid.TryPutInstruction(instruction))
                _instructions.Remove(instruction);
        if (_instructions.Count > 0)
        {
            Log.Error("JAI: printing finished with active instructions in queue!");
            _instructions.Clear();
        }
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
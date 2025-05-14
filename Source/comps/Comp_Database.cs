using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.statcollectors;
using ArchotechInfusions.ui;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Database : CompProperties
{
    public int MaxSpace = 1000;

    public CompProps_Database() => compClass = typeof(Comp_Database);
}

public class Comp_Database : CompBase_Grid<CompProps_Database>
{
    private List<Instruction> _modifiers = [];
    private float _spaceUsed;

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref _modifiers, "instructions", LookMode.Deep);
        RecalcSpaceUsed();
    }

    private void RecalcSpaceUsed() => _spaceUsed = _modifiers.Sum(m => m.Complexity);
    private float FreeSpace => Props.MaxSpace - _spaceUsed;

    public bool MakeDatabaseRecord(Instruction modifier)
    {
        if (!Power.PowerOn) return false;
        if (FreeSpace < modifier.Complexity) return false;

        _modifiers.Add(modifier);
        RecalcSpaceUsed();
        return true;
    }

    public IEnumerable<Instruction> Modifiers => _modifiers;

    public void RemoveModifier(Instruction modifier)
    {
        _modifiers.Remove(modifier);
        RecalcSpaceUsed();
    }

    public override string CompInspectStringExtra()
    {
        return $"Instructions stored: {_modifiers.Count}\nSpace used: {_spaceUsed:0.##} / {Props.MaxSpace}";
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "JAI.Database.ShowInstructions".Translate(),
            action = () =>
            {
                Find.WindowStack.TryRemove(typeof(CompDatabaseWindow));
                Find.WindowStack.Add(new CompDatabaseWindow(this));
            }
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action
            {
                defaultLabel = "Force make random",
                action = () =>
                {
                    _modifiers.Add(StatCollector.GenerateNewInstruction());
                    RecalcSpaceUsed();
                }
            };
        }
    }
}
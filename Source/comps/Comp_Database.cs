using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.instructions;
using ArchotechInfusions.statprocessor;
using ArchotechInfusions.ui;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Database : CompProperties
{
    public int MaxSpace = 1000;

    public CompProps_Database()
    {
        compClass = typeof(Comp_Database);
    }
}

[StaticConstructorOnStartup]
public class Comp_Database : CompBase_Grid<CompProps_Database>
{
    private static Texture2D showDbTex;

    private List<AInstruction> _modifiers = [];
    private float _spaceUsed;
    private float FreeSpace => Props.MaxSpace - _spaceUsed;

    public List<AInstruction> Modifiers => _modifiers;

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref _modifiers, "instructions", LookMode.Deep);
        RecalcSpaceUsed();
    }

    private void RecalcSpaceUsed()
    {
        _spaceUsed = _modifiers.Sum(m => Math.Abs(m.Complexity));
    }

    public bool MakeDatabaseRecord(AInstruction modifier)
    {
        if (!Power.PowerOn) return false;
        if (FreeSpace < Math.Abs(modifier.Complexity)) return false;

        _modifiers.Add(modifier);
        RecalcSpaceUsed();
        return true;
    }

    public bool TryRemoveInstruction(AInstruction modifier)
    {
        if (!Power.PowerOn) return false;
        return _modifiers.Remove(modifier);
    }

    public void RemoveModifier(AInstruction modifier)
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
        showDbTex ??= ContentFinder<Texture2D>.Get("ArchotechInfusions/UI/Gizmo/Database");
        yield return new Command_Action
        {
            defaultLabel = "JAI.Database.ShowInstructions".Translate(),
            icon = showDbTex,
            action = () =>
            {
                Find.WindowStack.TryRemove(typeof(CompDatabaseWindow));
                Find.WindowStack.Add(new CompDatabaseWindow(this));
            }
        };

        if (DebugSettings.ShowDevGizmos)
            yield return new Command_Action
            {
                defaultLabel = "Force make random",
                action = () =>
                {
                    _modifiers.Add(StatProcessor.GenerateInstruction());
                    RecalcSpaceUsed();
                }
            };
    }
}
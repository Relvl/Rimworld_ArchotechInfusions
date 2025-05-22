using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.instructions;
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

    private List<AInstruction> _instructions = [];
    private float _spaceUsed;
    private float FreeSpace => Props.MaxSpace - _spaceUsed;

    public List<AInstruction> Instructions => _instructions;

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref _instructions, nameof(Instructions), LookMode.Deep);
        RecalcSpaceUsed();
    }

    private void RecalcSpaceUsed()
    {
        _spaceUsed = _instructions.Sum(m => Math.Abs(m.Complexity));
    }

    public bool MakeDatabaseRecord(AInstruction modifier)
    {
        if (!Power.PowerOn) return false;
        if (FreeSpace < Math.Abs(modifier.Complexity)) return false;

        _instructions.Add(modifier);
        RecalcSpaceUsed();
        return true;
    }

    public bool TryRemoveInstruction(AInstruction modifier)
    {
        if (!Power.PowerOn) return false;
        return _instructions.Remove(modifier);
    }

    public void RemoveModifier(AInstruction modifier)
    {
        _instructions.Remove(modifier);
        RecalcSpaceUsed();
    }

    public override string CompInspectStringExtra()
    {
        return $"Instructions stored: {_instructions.Count}\nSpace used: {_spaceUsed:0.##} / {Props.MaxSpace}";
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
                defaultDesc = "Hold LeftControl to select generator",
                action = () =>
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                        Find.WindowStack.Add(new FloatMenu(FirstLevelDebugOptions().ToList()));
                    else
                        ForceAddInstruction(StatProcessor.GenerateInstruction());
                }
            };
    }

    private IEnumerable<FloatMenuOption> FirstLevelDebugOptions()
    {
        yield return new FloatMenuOption("Random instruction (same as natural)", () => ForceAddInstruction(StatProcessor.GenerateInstruction()));

        yield return new FloatMenuOption("Select special...", () =>
        {
            Find.WindowStack.Add(new FloatMenu(
                StatProcessor.GetSpecialGenerators()
                    .Select(g => new FloatMenuOption(g.Name, () => ForceAddInstruction(g.GenerateInstruction())))
                    .ToList()
            ));
        });

        yield return new FloatMenuOption("Select stat...", () =>
        {
            Find.WindowStack.Add(new FloatMenu(
                StatProcessor.GetStatGenerators()
                    .Select(g => new FloatMenuOption(g.Name, () => ForceAddInstruction(g.GenerateInstruction())))
                    .ToList()
            ));
        });
    }

    private void ForceAddInstruction(AInstruction instruction)
    {
        _instructions.Add(instruction);
        RecalcSpaceUsed();
    }
}
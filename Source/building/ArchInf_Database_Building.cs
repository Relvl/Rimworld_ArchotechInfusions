using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.building;

[StaticConstructorOnStartup]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ArchInf_Database_Building : AGridBuilding
{
    private static Texture2D ShowDbTex;

    private Comp_Database _comp;


    private List<AInstruction> _instructions = [];
    private float _spaceUsed;
    public Comp_Database Comp => _comp ??= GetComp<Comp_Database>();
    private float FreeSpace => Comp.Props.MaxSpace - _spaceUsed;

    public List<AInstruction> Instructions => _instructions;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref _instructions, nameof(Instructions), LookMode.Deep);
        RecalcSpaceUsed();
    }

    protected override void FillInspectStringExtra(StringBuilder sb)
    {
        sb.AppendLine($"\nInstructions stored: {_instructions.Count}\nSpace used: {_spaceUsed:0.##} / {Comp.Props.MaxSpace}");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        ShowDbTex ??= ContentFinder<Texture2D>.Get("ArchotechInfusions/UI/Gizmo/Database");
        yield return new Command_Action
        {
            defaultLabel = "JAI.Database.ShowInstructions".Translate(),
            icon = ShowDbTex,
            action = () =>
            {
                Find.WindowStack.TryRemove(typeof(DatabaseWindow));
                Find.WindowStack.Add(new DatabaseWindow(this));
            }
        };

        if (DebugSettings.ShowDevGizmos)
        {
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

            yield return new Command_Action
            {
                defaultLabel = "Show all stats",
                defaultDesc = "",
                action = () =>
                {
                    Find.WindowStack.TryRemove(typeof(StatListWindow));
                    Find.WindowStack.Add(new StatListWindow());
                }
            };
        }
    }

    private void RecalcSpaceUsed()
    {
        _spaceUsed = _instructions.Sum(m => Math.Abs(m.Complexity));
    }

    public bool MakeDatabaseRecord(AInstruction modifier)
    {
        if (!Grid.PowerOn) return false;
        if (FreeSpace < Math.Abs(modifier.Complexity)) return false;

        _instructions.Add(modifier);
        RecalcSpaceUsed();
        return true;
    }

    public bool TryRemoveInstruction(AInstruction modifier)
    {
        if (!Grid.PowerOn) return false;
        return _instructions.Remove(modifier);
    }

    public void RemoveModifier(AInstruction modifier)
    {
        _instructions.Remove(modifier);
        RecalcSpaceUsed();
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
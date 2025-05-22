using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ArchotechInfusions.instructions;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.injected;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Comp_ArchInfused : ThingComp
{
    private float _complexityCached;
    private string _descriptionPartCached;
    private string _labelAdditionCached;
    private List<InstructionStat> _instructions = [];

    private float ExtraComplexity;
    private float InitialIntegrity;
    private bool _isUnbreakable;

    public bool HasInstructions => _instructions.Count > 0 || _isUnbreakable;

    public float Integrity => InitialIntegrity - _complexityCached;

    public bool IsUnbreakable => _isUnbreakable;

    public List<InstructionStat> Instructions => _instructions;

    public void Apply(AInstruction instruction)
    {
        if (instruction is InstructionStat stat)
        {
            _instructions.Add(stat);
        }
        else if (instruction is InstructionIntegrity)
        {
            ExtraComplexity -= instruction.Complexity;
        }
        else
        {
            ExtraComplexity += instruction.Complexity;

            if (instruction is InstructionUnbreakable) _isUnbreakable = true;
            // todo extra durability
            // todo extra weight
            if (instruction is InstructionQuality)
            {
                // todo think about extra complexity for each QL
                var comp = parent.TryGetComp<CompQuality>();
                comp?.SetQuality(comp.Quality + 1, ArtGenerationContext.Colony);
            }
        }

        Invalidate();
    }

    private void Invalidate()
    {
        _instructions ??= [];
        _complexityCached = _instructions.Sum(s => s.Complexity) + ExtraComplexity;
        _descriptionPartCached = default;
        _labelAdditionCached = default;
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref _isUnbreakable, nameof(IsUnbreakable));
        Scribe_Values.Look(ref ExtraComplexity, nameof(ExtraComplexity));
        Scribe_Collections.Look(ref _instructions, "Instructions", LookMode.Deep);
        Invalidate();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (Mathf.Approximately(InitialIntegrity, default))
        {
            InitialIntegrity = parent.MaxHitPoints;
        }
    }
    
    public override bool AllowStackWith(Thing other) => !HasInstructions;

    public override string TransformLabel(string label)
    {
        var result = base.TransformLabel(label);
        if (HasInstructions)
        {
            if (_labelAdditionCached == default)
            {
                _labelAdditionCached = "";
                if (_instructions.Count > 0) _labelAdditionCached += $" +{_instructions.Count}";
                if (_isUnbreakable) _labelAdditionCached += " (Unbreakable)";
            }

            result += _labelAdditionCached;
        }

        return result;
    }

    public override string GetDescriptionPart()
    {
        var result = base.GetDescriptionPart();
        if (HasInstructions)
        {
            if (_descriptionPartCached == default)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Archotech Infusion:");
                sb.Append("\t").AppendLine("JAI.instruction.integrity.value".Translate(Integrity.ToString("0.00")));
                sb.AppendLine();
                foreach (var instruction in _instructions)
                {
                    sb.Append("\t").Append(instruction.Label).Append(": ");
                    instruction.RenderValue(sb);
                    sb.AppendLine();
                }

                sb.AppendLine();
                _descriptionPartCached = sb.ToString();
            }

            result += _descriptionPartCached;
        }

        return result;
    }
}
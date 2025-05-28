using System;
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
public class InstructionsComps : ThingComp
{
    private float _breakRandom;
    private float _complexityCached;
    private string _descriptionPartCached;

    private float _initialIntegrity;
    private List<InstructionStat> _instructions = [];
    private float _integrityQualityFactorCached;
    private bool _isUnbreakable;
    private string _labelAdditionCached;
    private float ExtraComplexity;

    public float Integrity => InitialIntegrity - _complexityCached;
    public float InitialIntegrity => _initialIntegrity * _integrityQualityFactorCached;

    public bool IsUnbreakable => _isUnbreakable;

    public List<InstructionStat> Instructions => _instructions;

    public void Apply(AInstruction instruction)
    {
        ExtraComplexity += instruction.Complexity;
        switch (instruction)
        {
            case InstructionStat stat:
                _instructions.Add(stat);
                break;
            case InstructionIntegrity:
                ExtraComplexity -= instruction.Complexity * 2f; // bcz already added
                break;
            case InstructionUnbreakable:
                _isUnbreakable = true;
                break;
            case InstructionQuality:
                var comp = parent.TryGetComp<CompQuality>();
                comp?.SetQuality(comp.Quality + 1, ArtGenerationContext.Colony);
                break;
        }

        Invalidate();
        GenLabel.ClearCache();
    }

    private void Invalidate()
    {
        _instructions ??= [];
        _complexityCached = _instructions.Sum(s => s.Complexity) + ExtraComplexity;
        _descriptionPartCached = default;
        _labelAdditionCached = default;
        _integrityQualityFactorCached = IntegrityQualityFactor();
        PostSpawnSetup(false);
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref _isUnbreakable, nameof(IsUnbreakable));
        Scribe_Values.Look(ref ExtraComplexity, nameof(ExtraComplexity));
        Scribe_Values.Look(ref _initialIntegrity, nameof(InitialIntegrity));
        Scribe_Values.Look(ref _breakRandom, "BreakRandom");
        Scribe_Collections.Look(ref _instructions, nameof(Instructions), LookMode.Deep);
        Invalidate();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (Mathf.Approximately(_initialIntegrity, default))
            _initialIntegrity = parent.MaxHitPoints;
        if (Mathf.Approximately(_breakRandom, default))
            _breakRandom = Rand.Value;
    }

    public override bool AllowStackWith(Thing other)
    {
        return _instructions.Count == 0 && !_isUnbreakable;
    }

    public override string TransformLabel(string label)
    {
        if (_labelAdditionCached == default)
        {
            if (Instructions.Count > 0)
                _labelAdditionCached = $"<color=#E57AFF>+{Instructions.Count}</color> " + base.TransformLabel(label);
            else
                _labelAdditionCached = base.TransformLabel(label);
        }

        return _labelAdditionCached;
    }

    public override string GetDescriptionPart()
    {
        var result = base.GetDescriptionPart();
        if (_instructions.Count > 0 || _isUnbreakable)
        {
            if (_descriptionPartCached == default || DebugSettings.ShowDevGizmos)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Archotech Infusion:");
                sb.Append("\t").Append("JAI.instruction.integrity.value".Translate(Integrity.ToString("0.##"))).AppendLine();

                if (DebugSettings.ShowDevGizmos)
                {
                    sb.Append("\tInitial integrity:").AppendLine(InitialIntegrity.ToString("0.##"));
                    sb.Append("\tExtra complexity:").AppendLine(ExtraComplexity.ToString("0.##"));
                }

                foreach (var instruction in _instructions)
                {
                    sb.Append("\t").Append(instruction.Label).Append(": ");
                    instruction.RenderValue(sb);
                    sb.AppendLine();
                }

                if (IsUnbreakable)
                    sb.Append("\t").Append("JAI.instruction.unbreakable".Translate());

                sb.AppendLine();
                _descriptionPartCached = sb.ToString();
            }

            result += _descriptionPartCached;
        }

        return result;
    }

    public (float damageAmount, float breakChance) GetDamageData(AInstruction instruction)
    {
        var damagePercent = instruction.Complexity / InitialIntegrity;
        var damageAmount = parent.MaxHitPoints * damagePercent;
        damageAmount = Math.Max(0, Math.Min(parent.HitPoints - 1, damageAmount));
        if (IsUnbreakable) damageAmount = 0;

        var breakChance = 0f;
        if (instruction.Complexity > 0 && Integrity < instruction.Complexity)
        {
            var newIntegrity = Integrity - instruction.Complexity;
            breakChance = -newIntegrity / InitialIntegrity / 2f; // todo curve
            breakChance = Mathf.Clamp(breakChance, 0.01f, 0.95f);
        }

        return (damageAmount, breakChance);
    }

    /// <summary>
    ///     Yes. Exactly. Anti save-scum. ;]
    /// </summary>
    public float TakeBreakRandom()
    {
        var result = _breakRandom;
        _breakRandom = Rand.Value;
        return result;
    }

    private float IntegrityQualityFactor()
    {
        if (parent.TryGetQuality(out var qc))
            return qc switch
            {
                QualityCategory.Awful => 0.5f,
                QualityCategory.Poor => 0.8f,
                QualityCategory.Normal => 1f,
                QualityCategory.Good => 1.2f,
                QualityCategory.Excellent => 1.5f,
                QualityCategory.Masterwork => 1.8f,
                QualityCategory.Legendary => 2f,
                _ => 1f
            };
        return 1f;
    }
}
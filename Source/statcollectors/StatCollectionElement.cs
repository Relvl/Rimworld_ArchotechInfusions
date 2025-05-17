using System;
using ArchotechInfusions.def;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ArchotechInfusions.statcollectors;

public class StatCollectionElement(StatDef statDef)
{
    public StatDef StatDef = statDef;
    public StatModDef Modifier = new();

    private float? _order;

    public bool AddUsed => Modifier.Add.x < Modifier.Add.y && Math.Abs(Modifier.Add.x - Modifier.Add.y) > 0.0001;
    public bool MulUsed => Modifier.Mul.x < Modifier.Mul.y && Math.Abs(Modifier.Mul.x - Modifier.Mul.y) > 0.0001;
    public bool IsPassive => !AddUsed && !MulUsed;

    public float Order()
    {
        if (_order is { } d) return d;
        _order = 1f;
        _order += Math.Abs(Modifier.Add.x + Modifier.Add.y) + Math.Abs(Modifier.Mul.x + Modifier.Mul.y);
        _order *= Mathf.Pow((float)Modifier.TechLevel, 1.2f);
        return ((float)_order).PercentOfRange(0.001f, 200);
    }

    public StatCollectionElement UpdateTypeFilter(InstructionTarget filter)
    {
        Modifier.TypeFilter |= filter;
        return this;
    }

    public StatCollectionElement FillThing(ThingDef thingDef)
    {
        Modifier.TechLevel = Modifier.TechLevel > thingDef.techLevel ? thingDef.techLevel : Modifier.TechLevel;
        return this;
    }

    public StatCollectionElement FillOffsets(StatModifier modifier)
    {
        Modifier.Add.x = Mathf.Min(Modifier.Add.x, modifier.value);
        Modifier.Add.y = Mathf.Max(Modifier.Add.y, modifier.value);
        Modifier.Add.z = 0.5f;
        Modifier.Mul.x = Mathf.Min(Modifier.Mul.x, modifier.value / 10);
        Modifier.Mul.y = Mathf.Max(Modifier.Mul.y, modifier.value / 10);
        Modifier.Mul.z = 0.5f;
        return this;
    }

    public void UpdateFrom(StatCollectionElement slave)
    {
        if (Modifier.TechLevel > slave.Modifier.TechLevel) Modifier.TechLevel = slave.Modifier.TechLevel;

        Modifier.Add.x = Mathf.Min(Modifier.Add.x, slave.Modifier.Add.x);
        Modifier.Add.y = Mathf.Max(Modifier.Add.y, slave.Modifier.Add.y);
        Modifier.Mul.x = Mathf.Min(Modifier.Mul.x, slave.Modifier.Mul.x);
        Modifier.Mul.y = Mathf.Max(Modifier.Mul.y, slave.Modifier.Mul.y);

        Modifier.TypeFilter |= slave.Modifier.TypeFilter;

        if (Modifier.IsNegated != slave.Modifier.IsNegated)
        {
            Log.Warning($"ArchInf: merge stats with different Negated param - why? StatDef = {StatDef.defName}");
            Modifier.IsNegated |= slave.Modifier.IsNegated;
        }
    }

    public Instruction Generate()
    {
        var result = new Instruction { TypeFilter = Modifier.TypeFilter, TechLevel = Modifier.TechLevel, Def = StatDef };
        Modifier.Generate(out result.Value, out result.Type, out result.Complexity);
        return result;
    }
}
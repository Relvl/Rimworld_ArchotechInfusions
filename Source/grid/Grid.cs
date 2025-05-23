using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps;
using ArchotechInfusions.comps.comp_base;
using ArchotechInfusions.instructions;
using UnityEngine;
using Verse;

namespace ArchotechInfusions;

public class Grid
{
    private readonly Dictionary<Type, List<IBaseGridComp<CompPropertiesBase_Grid>>> _gridCache = new();

    public readonly Guid Guid = Guid.NewGuid();
    public readonly List<IBaseGridComp<CompPropertiesBase_Grid>> Members = [];

    private int _skipAccumBalance;

    public void OnTick()
    {
        BalanceAccumulators();
    }

    public void AddMember(IBaseGridComp<CompPropertiesBase_Grid> member)
    {
        member.Grid = this;
        Members.Add(member);

        foreach (var thingComp in member.Parent.AllComps)
        {
            if (thingComp is not IBaseGridComp<CompPropertiesBase_Grid> gridComp) continue;
            if (!_gridCache.ContainsKey(gridComp.GetType())) _gridCache[gridComp.GetType()] = [];
            _gridCache[thingComp.GetType()].Add(gridComp);
        }
    }

    public void Invalidate()
    {
        _gridCache.Clear();
        Members.Clear();
    }

    public List<T> Get<T>() where T : IBaseGridComp<CompPropertiesBase_Grid>
    {
        if (!_gridCache.ContainsKey(typeof(T))) _gridCache[typeof(T)] = [];
        return _gridCache[typeof(T)].Cast<T>().ToList();
    }

    public float GetTotalEnergy()
    {
        return Get<Comp_Accumulator>().Sum(a => a.Stored);
    }

    public float GetTotalArchite()
    {
        return Get<Comp_ArchiteContainer>().Sum(a => a.Stored);
    }

    public void ConsumeEnergy(ref float wantedAmount)
    {
        foreach (var accumulator in Get<Comp_Accumulator>())
        {
            accumulator.Consume(ref wantedAmount);
            if (wantedAmount <= 0) break;
        }
    }

    public void ConsumeArchite(ref float wantedAmount)
    {
        foreach (var container in Get<Comp_ArchiteContainer>().OrderBy(c => c.Stored))
        {
            container.Consume(ref wantedAmount);
            if (wantedAmount <= 0) break;
        }
    }

    public bool TryPutInstruction(AInstruction instruction)
    {
        if (instruction == default) return false;
        foreach (var database in Get<Comp_Database>())
            if (database.MakeDatabaseRecord(instruction))
                return true;
        return false;
    }

    public bool TryRemoveInstruction(AInstruction instruction)
    {
        if (instruction == default) return false;
        foreach (var database in Get<Comp_Database>())
            if (database.TryRemoveInstruction(instruction))
                return true;
        return false;
    }

    private void BalanceAccumulators()
    {
        if (--_skipAccumBalance > 0)
            return;

        var accumulators = Get<Comp_Accumulator>().ToList();
        if (accumulators.Count <= 1)
        {
            _skipAccumBalance = 50;
            return;
        }

        if (!accumulators.Where(a => !a.MarkedToDischarge && !a.IsFull).TryMinBy(a => a.Stored, out var toCharge))
        {
            _skipAccumBalance = 25;
            return;
        }

        if (accumulators.Where(a => a.MarkedToDischarge && a.Stored > 0).TryMaxBy(a => a.Stored, out var toDischarge))
        {
            var toTransfer = toDischarge.Props.MaxStored * 0.005f;
            toCharge.Stored += toTransfer;
            toDischarge.Stored -= toTransfer;
            return;
        }

        if (accumulators.Where(a => a.Stored > 0).TryMaxBy(a => a.Stored, out toDischarge))
        {
            var diff = toDischarge.Stored - toCharge.Stored;
            if (diff < 1f)
            {
                _skipAccumBalance = 25;
                return;
            }

            var toTransfer = Mathf.Max(diff * 0.01f, 1f);
            toCharge.Stored += toTransfer;
            toDischarge.Stored -= toTransfer;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.building;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.instructions;
using UnityEngine;
using Verse;

namespace ArchotechInfusions;

public class Grid
{
    private readonly Dictionary<Type, List<AGridBuilding>> _gridCache = new();

    public readonly Guid Guid = Guid.NewGuid();
    public readonly List<AGridBuilding> Members = [];

    private int _skipAccumBalance;

    public bool PowerOn => true; // todo

    public void OnTick()
    {
        BalanceAccumulators();
    }

    public void AddMember(AGridBuilding member)
    {
        member.Grid = this;
        Members.Add(member);
        if (!_gridCache.ContainsKey(member.GetType())) _gridCache[member.GetType()] = [];
        _gridCache[member.GetType()].Add(member);
    }

    public void Invalidate()
    {
        _gridCache.Clear();
        Members.Clear();
    }

    public List<T> Get<T>() where T : AGridBuilding
    {
        if (!_gridCache.ContainsKey(typeof(T))) _gridCache[typeof(T)] = [];
        return _gridCache[typeof(T)].Cast<T>().ToList();
    }

    public float GetTotalEnergy()
    {
        return Get<ArchInf_Accumulator_Building>().Sum(a => a.Stored);
    }

    public float GetTotalArchite()
    {
        return Get<ArchInf_Container_Building>().Sum(a => a.Stored);
    }

    public void ConsumeEnergy(ref float wantedAmount)
    {
        foreach (var accumulator in Get<ArchInf_Accumulator_Building>())
        {
            accumulator.Consume(ref wantedAmount);
            if (wantedAmount <= 0) break;
        }
    }

    public void ConsumeArchite(ref float wantedAmount)
    {
        foreach (var container in Get<ArchInf_Container_Building>().OrderBy(c => c.Stored))
        {
            container.Consume(ref wantedAmount);
            if (wantedAmount <= 0) break;
        }
    }

    public bool TryPutInstruction(AInstruction instruction)
    {
        if (instruction == default) return false;
        foreach (var database in Get<ArchInf_Database_Building>())
            if (database.MakeDatabaseRecord(instruction))
                return true;
        return false;
    }

    public bool TryRemoveInstruction(AInstruction instruction)
    {
        if (instruction == default) return false;
        foreach (var database in Get<ArchInf_Database_Building>())
            if (database.TryRemoveInstruction(instruction))
                return true;
        return false;
    }

    private void BalanceAccumulators()
    {
        if (--_skipAccumBalance > 0)
            return;

        var accumulators = Get<ArchInf_Accumulator_Building>().ToList();
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
            var toTransfer = toDischarge.Comp.Props.MaxStored * 0.005f;
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
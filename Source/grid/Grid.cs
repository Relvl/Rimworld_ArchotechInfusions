using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps;
using ArchotechInfusions.instructions;
using Verse;

namespace ArchotechInfusions.grid;

public class Grid(GridMapComponent mapComponent, string gridType)
{
    private readonly Dictionary<Type, List<ThingComp>> _compsCache = new();

    private readonly Dictionary<Type, List<GridMemberComp>> _parentTypeCache = new();
    public readonly string GridType = gridType;
    public readonly Guid Guid = Guid.NewGuid();
    public readonly GridMapComponent MapComponent = mapComponent;
    public readonly List<GridMemberComp> Members = [];

    public void OnTick()
    {
    }

    public void AddMember(GridMemberComp member)
    {
        member.Grid = this;
        Members.Add(member);

        var type = member.parent.GetType();
        if (!_parentTypeCache.ContainsKey(type)) _parentTypeCache[type] = [];
        _parentTypeCache[type].Add(member);

        foreach (var thingComp in member.parent.AllComps)
        {
            if (thingComp is GridMemberComp) continue;
            if (!_compsCache.ContainsKey(thingComp.GetType())) _compsCache[thingComp.GetType()] = [];
            _compsCache[thingComp.GetType()].Add(thingComp);
        }
    }

    public List<T> GetComps<T>() where T : ThingComp
    {
        if (!_compsCache.ContainsKey(typeof(T))) _compsCache[typeof(T)] = [];
        return _compsCache[typeof(T)].Cast<T>().ToList();
    }

    public float GetTotalCharge()
    {
        return GetComps<Comp_Accumulator>().Sum(a => a.Stored);
    }

    public float GetTotalArchite()
    {
        return GetComps<Comp_ArchiteContainer>().Sum(a => a.Stored);
    }

    public void ConsumeEnergy(ref float wantedAmount)
    {
        foreach (var accumulator in GetComps<Comp_Accumulator>())
        {
            // todo distribute consumption! not from just first one!
            accumulator.Consume(ref wantedAmount);
            if (wantedAmount <= 0) break;
        }
    }

    public void ConsumeArchite(ref float wantedAmount)
    {
        foreach (var container in GetComps<Comp_ArchiteContainer>())
        {
            // todo distribute consumption! not from just first one!
            container.Consume(ref wantedAmount);
            if (wantedAmount <= 0) break;
        }
    }

    public bool TryPutInstruction(AInstruction instruction)
    {
        if (instruction == default) return false;
        foreach (var database in GetComps<Comp_Database>())
            if (database.MakeDatabaseRecord(instruction))
                return true;
        return false;
    }

    public bool TryRemoveInstruction(AInstruction instruction)
    {
        if (instruction == default) return false;
        foreach (var database in GetComps<Comp_Database>())
            if (database.TryRemoveInstruction(instruction))
                return true;
        return false;
    }
}
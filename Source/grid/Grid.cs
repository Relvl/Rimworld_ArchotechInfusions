using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ArchotechInfusions.grid;

public class Grid
{
    public readonly Guid Guid;
    public readonly GridMapComponent MapComponent;
    public readonly string GridType;
    public readonly List<GridMemberComp> Members = new();

    private readonly Dictionary<Type, List<GridMemberComp>> _parentTypeCache = new();
    private readonly Dictionary<Type, List<ThingComp>> _compsCache = new();

    public Grid(GridMapComponent mapComponent, string gridType)
    {
        Guid = Guid.NewGuid();
        MapComponent = mapComponent;
        GridType = gridType;
    }

    public void OnTick()
    {
    }

    public void AddMember(GridMemberComp member)
    {
        member.Grid = this;
        Members.Add(member);

        var type = member.parent.GetType();
        if (!_parentTypeCache.ContainsKey(type)) _parentTypeCache[type] = new List<GridMemberComp>();
        _parentTypeCache[type].Add(member);

        foreach (var thingComp in member.parent.AllComps)
        {
            if (thingComp is GridMemberComp) continue;
            if (!_compsCache.ContainsKey(thingComp.GetType())) _compsCache[thingComp.GetType()] = new List<ThingComp>();
            _compsCache[thingComp.GetType()].Add(thingComp);
        }
    }

    public List<GridMemberComp> GetMembersOfParent(Type type)
    {
        if (!_parentTypeCache.ContainsKey(type)) _parentTypeCache[type] = new List<GridMemberComp>();
        return _parentTypeCache[type].ToList();
    }

    public List<T> GetComps<T>() where T : ThingComp
    {
        if (!_compsCache.ContainsKey(typeof(T))) _compsCache[typeof(T)] = new List<ThingComp>();
        return _compsCache[typeof(T)].Cast<T>().ToList();
    }
}
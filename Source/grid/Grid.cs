using System;
using System.Collections.Generic;

namespace ArchotechInfusions.grid;

public class Grid
{
    public readonly Guid Guid;
    public readonly GridMapComponent MapComponent;
    public readonly string GridType;
    public readonly List<GridMemberComp> Members = new();

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
    }
}
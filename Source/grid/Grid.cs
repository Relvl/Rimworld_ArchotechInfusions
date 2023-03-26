using System;
using System.Collections.Generic;

namespace ArchotechInfusions.grid;

public class Grid
{
    public readonly Guid Guid;
    public readonly GridMapComponent Comp;
    public List<GridMemberComp> Members = new();

    public Grid(GridMapComponent comp)
    {
        Guid = Guid.NewGuid();
        Comp = comp;
    }

    public void OnTick()
    {
    }

    public void AddMember(GridMemberComp member)
    {
        member.Grid = this;
    }
}
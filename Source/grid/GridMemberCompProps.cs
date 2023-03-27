using ArchotechInfusions.grid.graphic;
using Verse;

namespace ArchotechInfusions.grid;

// ReSharper disable once ClassNeverInstantiated.Global -- reflective
public class GridMemberCompProps : CompProperties
{
    public string GridType;
    public GridVisibility Visibility = GridVisibility.Allways;
    
    public GridMemberCompProps() => compClass = typeof(GridMemberComp);
}
using Verse;

namespace ArchotechInfusions;

public static class GridExtensions
{
    /// <summary>
    ///     Returns current map's grid component. Don't need to be cached all of it, it needs only on current map?
    /// </summary>
    public static GridMapComponent ArchInfGrid(this Map map)
    {
        return GridMapComponent.GetInstance(map);
    }
}
using Verse;

namespace ArchotechInfusions.grid;

public static class GridExtensions
{
    /// <summary>
    ///  Returns current map's grid component. Dont need to cached all of it, it needs only on current map.
    /// </summary>
    public static GridMapComponent LightGrid(this Map map) => GridMapComponent.GetInstance(map);
}
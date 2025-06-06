using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.graphic;
using UnityEngine;
using Verse;

namespace ArchotechInfusions;

[StaticConstructorOnStartup]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GridMapComponent : MapComponent
{
    public static readonly GraphicGridOverlay Overlay = new(
        GraphicDatabase.Get<Graphic_Single>( //
            "ArchotechInfusions/Things/GridOverlay_Atlas",
            ShaderDatabase.MetaOverlay,
            Vector2.one,
            new Color32(159, 217, 60, 190)
        )
    );

    public static Grid GridToDebug;

    private static (int, GridMapComponent) _cachedGridComponent = (-1, null);

    private readonly List<Grid> _grids = [];
    private readonly Dictionary<IntVec3, AGridBuilding> _members = new();

    public GridMapComponent(Map map) : base(map)
    {
        _cachedGridComponent = (map.uniqueID, this);
    }

    public override void MapComponentTick()
    {
        _grids.ForEach(g => g.OnTick());
    }

    /// <summary>
    ///     After all things spawned
    /// </summary>
    public override void FinalizeInit()
    {
        RebuildGrids();
    }

    /// <summary>
    ///     After FinalizeInit
    /// </summary>
    public override void MapGenerated()
    {
        RebuildGrids();
    }

    /// <summary>
    ///     ??? Why don't called when exit to main menu?
    /// </summary>
    public override void MapRemoved()
    {
        _cachedGridComponent = (-1, null);
        base.MapRemoved();
    }

    public void Register(AGridBuilding member, bool respawningAfterLoad)
    {
        try
        {
            foreach (var c in member.OccupiedRect())
                _members.Add(c, member);
        }
        catch (Exception)
        {
            Log.Error("ArchInf: Trying to register grid member to positions that allready occupied by another member... Grid components should not overlap!");
            throw;
        }

        if (!respawningAfterLoad)
            // todo подумать как добавить к существующей сети без полной регенерации, если не после респавна
            // todo возможно, надо обыскать все прилежащие к parent.OccupiedRect() клетки на предмет _registeredMembers
            // todo а если нету - то регистрировать новую сеть
            RebuildGrids();
    }

    public void Unregister(AGridBuilding member)
    {
        foreach (var c in member.OccupiedRect())
            _members.Remove(c);
        RebuildGrids();
    }

    private void RebuildGrids()
    {
        _grids.ForEach(g => g.Invalidate());
        _grids.Clear();
        foreach (var (_, member) in _members)
            member.Grid = null;

        var watchdog = 1000;
        while (true)
        {
            if (--watchdog <= 0) throw new Exception("ArchInf: too many RebuildGrids loops... Something went wrong!");

            var gridStarter = _members.Values.FirstOrDefault(m => m.Grid == null);
            if (gridStarter is null) break;

            gridStarter.Grid = new Grid();
            gridStarter.Grid.AddMember(gridStarter);
            _grids.Add(gridStarter.Grid);

            map.floodFiller.FloodFill(gridStarter.Position, PassCheck, PassProcessor);
            continue;

            bool PassCheck(IntVec3 c)
            {
                var another = _members.TryGetValue(c);
                if (another is null) return false;
                if (another == gridStarter) return true;
                if (another.Grid != null) return another.Grid == gridStarter.Grid;
                gridStarter.Grid.AddMember(another);
                return true;
            }

            void PassProcessor(IntVec3 c)
            {
            }
        }
    }

    public bool IsSameGrid(IntVec3 c, AGridBuilding comp)
    {
        return _members.ContainsKey(c) && _members[c].Grid == comp.Grid;
    }

    public void RenderOverlay(SectionLayer layer, IntVec3 c)
    {
        if (_members.TryGetValue(c, out var member))
        {
            if (GridToDebug is not null && member.Grid.Guid != GridToDebug.Guid) return;
            Overlay.Print(layer, member, 0.0f);
        }
    }

    public bool ShouldConnect(IntVec3 c, AGridBuilding member)
    {
        return member != null && _members.ContainsKey(c);
    }

    public IEnumerable<T> Get<T>() where T : AGridBuilding
    {
        foreach (var grid in _grids)
        foreach (var comp in grid.Get<T>())
            yield return comp;
    }

    public static GridMapComponent GetInstance(Map map)
    {
        if (_cachedGridComponent.Item1 != map.uniqueID)
        {
            _cachedGridComponent = (map.uniqueID, map.GetComponent<GridMapComponent>());
            Log.Message("ArchInf: create grid component cache");
        }

        if (_cachedGridComponent.Item2 is null) throw new Exception($"Map '{map.uniqueID}' hasn't ArchInfGridComponent - maybe not proper generated?");
        return _cachedGridComponent.Item2;
    }
}
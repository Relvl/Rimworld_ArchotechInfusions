using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.grid.graphic;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid;

[StaticConstructorOnStartup]
// ReSharper disable once UnusedType.Global -- reflective: Verse.Map:FillComponents
// ReSharper disable once ClassNeverInstantiated.Global
public class GridMapComponent : MapComponent
{
    // todo def-generated
    private static readonly GraphicGridOverlay Overlay = new(
        GraphicDatabase.Get<Graphic_Single>( //
            "ArchotechInfusions/Things/GridOverlay_Atlas",
            ShaderDatabase.MetaOverlay,
            Vector2.one,
            new Color32(159, 217, 60, 190)
        )
    );

    public static Grid DebudGrid;

    private static (int, GridMapComponent) _cachedGridComponent = (-1, null);

    private readonly List<Grid> _grids = [];
    private readonly Dictionary<IntVec3, GridMemberComp> _registeredMembers = new();

    public GridMapComponent(Map map) : base(map)
    {
        _cachedGridComponent = (map.uniqueID, this);
    }

    public override void MapComponentTick() => _grids.ForEach(g => g.OnTick());

    /// <summary>
    /// After all things spawned
    /// </summary>
    public override void FinalizeInit()
    {
        RebuildGrids();
    }

    /// <summary>
    /// After FinalizeInit
    /// </summary>
    public override void MapGenerated()
    {
        RebuildGrids();
    }

    /// <summary>
    /// ??? Why don't called when exit to main menu?
    /// </summary>
    public override void MapRemoved()
    {
        _cachedGridComponent = (-1, null);
        base.MapRemoved();
    }

    public void Register(GridMemberComp member, bool respawningAfterLoad)
    {
        try
        {
            foreach (var c in member.parent.OccupiedRect()) _registeredMembers.Add(c, member);
        }
        catch (Exception)
        {
            Log.Error("ArchInf: Trying to register grid member to positions that allready occupied by another member... Grid components should not overlap!");
            throw;
        }

        if (!respawningAfterLoad)
        {
            // todo подумать как добавить к существующей сети без полной регенерации, если не после респавна
            // todo возможно, надо обыскать все прилежащие к parent.OccupiedRect() клетки на предмет _registeredMembers
            // todo а если нету - то регистрировать новую сеть
            RebuildGrids();
        }
    }

    public void Unregister(GridMemberComp member)
    {
        foreach (var c in member.parent.OccupiedRect()) _registeredMembers.Remove(c);
        RebuildGrids();
    }

    private void RebuildGrids()
    {
        _grids.Clear();
        foreach (var (_, member) in _registeredMembers) member.Grid = null;

        var watchdog = 1000;

        while (true)
        {
            if (--watchdog <= 0) throw new Exception("ArchInf: too many RebuildGrids loops... Something went wrong!");

            var initial = _registeredMembers.Values.FirstOrDefault(m => m.Grid == null);
            if (initial is null) break;
            initial.Grid = InitializeGrid(initial);
            _grids.Add(initial.Grid);

            bool PassCheck(IntVec3 c)
            {
                // Ищем другого свободного члена сети
                var another = _registeredMembers.TryGetValue(c);
                if (another is null) return false;
                if (another == initial) return true;
                if (another.Props.GridType != initial.Props.GridType) return false;
                if (another.Grid != null) return another.Grid == initial.Grid;
                // Добавляем его к той же сети
                initial.Grid.AddMember(another);
                return true;
            }

            map.floodFiller.FloodFill(initial.parent.Position, PassCheck, _ => { });
        }

        if (Prefs.DevMode) Log.Warning($"ArchInf: rebuilded {_grids.Count} grids");
    }

    private Grid InitializeGrid(GridMemberComp member)
    {
        var grid = new Grid(this, member.Props.GridType);
        grid.AddMember(member);
        return grid;
    }

    public bool IsSameGrid(IntVec3 c, GridMemberComp comp) => _registeredMembers.ContainsKey(c) && _registeredMembers[c].Grid == comp.Grid;

    public void RenderOverlay(SectionLayer layer, IntVec3 c)
    {
        if (_registeredMembers.ContainsKey(c))
        {
            var comp = _registeredMembers[c];
            if (DebudGrid is not null && comp.Grid.Guid != DebudGrid.Guid) return;
            Overlay.Print(layer, comp.parent, 0.0f);
        }
    }

    public bool ShouldConnect(IntVec3 c, GridMemberComp comp)
    {
        return comp != null && _registeredMembers.ContainsKey(c) && _registeredMembers[c].Props.GridType == comp.Props.GridType;
    }

    public IEnumerable<T> GetComps<T>() where T : ThingComp
    {
        foreach (var grid in _grids)
        foreach (var comp in grid.GetComps<T>())
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
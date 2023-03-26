using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid;

[StaticConstructorOnStartup]
// ReSharper disable once UnusedType.Global -- reflective: Verse.Map:FillComponents
// ReSharper disable once ClassNeverInstantiated.Global
public class GridMapComponent : MapComponent
{
    // todo def-generated
    private static readonly GridLinkedOverlay Overlay = new(
        GraphicDatabase.Get<Graphic_Single>( //
            "ArchotechInfusions/Things/GridOverlay_Atlas",
            ShaderDatabase.MetaOverlay,
            Vector2.one,
            new Color32(159, 217, 60, 190)
        )
    );

    private static (int, GridMapComponent) _cachedGridComponent = (-1, null);

    private readonly List<Grid> _grids = new();
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
        Log.Warning("ArchInf: FinalizeInit");
        RebuildGrids();
    }

    /// <summary>
    /// After FinalizeInit
    /// </summary>
    public override void MapGenerated()
    {
        Log.Warning("ArchInf: MapGenerated");
        RebuildGrids();
    }

    /// <summary>
    /// ??? Why don't called when exit to main menu?
    /// </summary>
    public override void MapRemoved()
    {
        Log.Warning("ArchInf: map removed");
        _cachedGridComponent = (-1, null);
        base.MapRemoved();
    }

    public void Register(GridMemberComp member, bool respawningAfterLoad)
    {
        Log.Warning($"ArchInf: Register: {member.parent.ThingID} (grid type: {member.Props.GridType})");

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
        Log.Warning($"ArchInf: Unregister: {member.parent.Label}");
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
            if (Prefs.DevMode) Log.Message($"ArchInf: deflating initial grid {initial.Grid.Guid} from member {initial.parent.ThingID}");

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
                if (Prefs.DevMode) Log.Message($"ArchInf: added another member to grid {initial.Grid.Guid} = {another.parent.ThingID}");
                return true;
            }

            map.floodFiller.FloodFill(initial.parent.Position, PassCheck, _ => { });
        }

        if (Prefs.DevMode) Log.Warning($"ArchInf: rebuilded {_grids.Count} grids");
    }

    private Grid InitializeGrid(GridMemberComp member)
    {
        var grid = new Grid(this);
        grid.AddMember(member);
        return grid;
    }

    public bool IsSameGrid(IntVec3 c, GridMemberComp comp) => _registeredMembers.ContainsKey(c) && _registeredMembers[c].Grid == comp.Grid;

    public void RenderOverlay(SectionLayer layer, IntVec3 c)
    {
        if (_registeredMembers.ContainsKey(c))
        {
            Overlay.Print(layer, _registeredMembers[c].parent, 0.0f);
        }
    }

    public bool ShouldConnect(IntVec3 c, GridMemberComp comp)
    {
        return comp != null && _registeredMembers.ContainsKey(c) && _registeredMembers[c].Props.GridType == comp.Props.GridType;
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
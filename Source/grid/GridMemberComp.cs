using System.Linq;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid;

[StaticConstructorOnStartup]
public class GridMemberComp : ThingComp
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

    public Grid Grid { get; set; }

    public GridMemberCompProps Props => (GridMemberCompProps)props;

    public override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        foreach (var vec in parent.OccupiedRect())
        foreach (var thing in vec.GetThingList(parent.Map).ToList())
            if (thing != parent && thing.TryGetComp<GridMemberComp>() != null)
                thing.Destroy(); // todo drop resources

        parent.Map.LightGrid().Register(this, respawningAfterLoad);

        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void PostDeSpawn(Map map)
    {
        map.LightGrid().Unregister(this);
        base.PostDeSpawn(map);
    }

    public void PrintForGrid(SectionLayer layer) => Overlay.Print(layer, parent, 0.0f);
}
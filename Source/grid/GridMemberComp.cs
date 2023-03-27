using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.grid.graphic;
using Verse;

namespace ArchotechInfusions.grid;

[StaticConstructorOnStartup]
public class GridMemberComp : ThingComp
{
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

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: show grid info",
                action = () =>
                {
                    Find.WindowStack.TryRemove(typeof(GridInfoWindow));
                    Find.WindowStack.Add(new GridInfoWindow(this));
                }
            };
        }
    }
}
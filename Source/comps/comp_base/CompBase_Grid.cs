using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.graphic;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class CompBase_Grid<TProperties> : ThingComp, IBaseGridComp<TProperties> where TProperties : CompPropertiesBase_Grid
{
    private CompPowerTrader _power;
    public CompPowerTrader Power => _power ??= Parent.TryGetComp<CompPowerTrader>();

    public TProperties Props => props as TProperties;
    public Grid Grid { get; set; }
    public ThingWithComps Parent => parent;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        // todo what's this?
        /*foreach (var vec in Parent.OccupiedRect())
        foreach (var thing in vec.GetThingList(Parent.Map).ToList())
            if (thing != Parent && thing.TryGetComp<Comp_GridVisibility>() != null)
                thing.Destroy(); // todo drop resources*/

        Parent.Map.ArchInfGrid().Register(this, respawningAfterLoad);
    }

    public override void PostDeSpawn(Map map)
    {
        map.ArchInfGrid().Unregister(this);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
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
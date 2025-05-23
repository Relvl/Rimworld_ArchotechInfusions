using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.graphic;
using RimWorld;
using Verse;

namespace ArchotechInfusions.building.proto;

public abstract class AddInf_Building : Building
{
    protected CompPowerTrader Power { get; private set; }
    public Grid Grid { get; set; }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

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

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        // todo what's this?
        /*foreach (var vec in Parent.OccupiedRect())
        foreach (var thing in vec.GetThingList(Parent.Map).ToList())
            if (thing != Parent && thing.TryGetComp<Comp_GridVisibility>() != null)
                thing.Destroy(); // todo drop resources*/

        Power = GetComp<CompPowerTrader>();
        if (Power is null && this is not ArchInf_Loom_Building)
            throw new Exception($"ArchInf: {GetType().FullName} can't work without {nameof(CompPowerTrader)}");

        Map.ArchInfGrid().Register(this, respawningAfterLoad);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        Map.ArchInfGrid().Unregister(this);
        base.DeSpawn(mode);
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder(base.GetInspectString()).AppendLine();
        if (!Power.PowerOn)
            sb.AppendLine("JAI.Error.PoweredOff".Translate());
        FillInspectStringExtra(sb);

        return sb.TrimEnd().ToString();
    }

    protected virtual void FillInspectStringExtra(StringBuilder sb)
    {
    }
}
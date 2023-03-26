using Verse;

namespace ArchotechInfusions.place_worker;

// ReSharper disable once UnusedType.Global -- reflective
public class PlaceWorkerLoom : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        return base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
    }
}
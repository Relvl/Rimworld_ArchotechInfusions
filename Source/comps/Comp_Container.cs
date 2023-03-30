using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Container : CompProperties
{
    public int MaxStored = 10000;
    public List<ItemData> AvailableItems = new();

    public CompProps_Container() => compClass = typeof(Comp_Container);
}

public class Comp_Container : CompBase_Membered<CompProps_Container>
{
    private ItemData _minContainer;
    private ItemData _maxContainer;
    private float _stored;
    private ItemData _maxUnloadableContainer;

    public float Stored
    {
        get => _stored;
        set
        {
            _stored = Math.Max(0, Math.Min(value, Props.MaxStored));
            var avData = Props.AvailableItems.Where(d => d.ArchiteCount < value).ToList();
            _maxUnloadableContainer = avData.Count > 0 ? avData.MaxBy(d => d.ArchiteCount) : null;
        }
    }

    public override void Initialize(CompProperties p)
    {
        base.Initialize(p);
        _minContainer = Props.AvailableItems.MinBy(c => c.ArchiteCount);
        _maxContainer = Props.AvailableItems.MaxBy(c => c.ArchiteCount);
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref _stored, "stored");
        Stored = _stored;
    }

    public float FreeSpace => Props.MaxStored - Stored;

    public IEnumerable<Thing> GetAvailableFuels(Map map) => Props.AvailableItems.Where(f => f.ArchiteCount < FreeSpace).SelectMany(f => map.listerThings.ThingsOfDef(f.ThingDef));

    public bool IsThingAllowedAsFuel(Thing thing) => Props.AvailableItems.Any(f => f.ThingDef == thing.def && f.ArchiteCount < FreeSpace);

    public bool CanStoreMore() => Power.PowerOn && FreeSpace >= _minContainer.ArchiteCount;

    public void InsertFuel(Thing fuel)
    {
        var data = Props.AvailableItems.First(i => i.ThingDef == fuel.def);
        Stored += data.ArchiteCount;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Archite stored: {Stored:0.##}µU");
        return sb.TrimEnd().ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "Unload",
            disabled = _maxUnloadableContainer is null,
            disabledReason = "Not enought archite to fill any container",
            action = () =>
            {
                var thing = _maxUnloadableContainer.ThingDef.MadeFromStuff //
                    ? ThingMaker.MakeThing(_maxUnloadableContainer.ThingDef, GenStuff.DefaultStuffFor(_maxUnloadableContainer.ThingDef))
                    : ThingMaker.MakeThing(_maxUnloadableContainer.ThingDef);
                GenDrop.TryDropSpawn(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near, out var dropped);
                if (dropped != null) Stored -= _maxUnloadableContainer.ArchiteCount;

                SoundDefOf.DropPod_Open.PlayOneShot(parent);
            }
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "-10%", action = () => Stored -= Props.MaxStored * 0.1f };
            yield return new Command_Action { defaultLabel = "+10%", action = () => Stored += Props.MaxStored * 0.1f };
            yield return new Command_Action { defaultLabel = "Full", action = () => Stored = Props.MaxStored };
        }
    }
}

public class ItemData
{
    public ThingDef ThingDef;
    public int ArchiteCount;
}
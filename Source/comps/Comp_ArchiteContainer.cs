using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchotechInfusions.comps.comp_base;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.comps;

public class CompProps_ArchiteContainer : CompPropertiesBase_Grid
{
    public List<ItemData> AvailableItems = [];
    public int MaxStored = 10000;

    public CompProps_ArchiteContainer()
    {
        compClass = typeof(Comp_ArchiteContainer);
    }
}

[StaticConstructorOnStartup]
public class Comp_ArchiteContainer : CompBase_Grid<CompProps_ArchiteContainer>
{
    private bool _allowAutoRefuel = true;

    private ItemData _maxUnloadableContainer;
    private ItemData _minContainer;

    /// <summary>
    /// </summary>
    private float _stored;

    public float Stored
    {
        get => _stored;
        private set
        {
            _stored = Math.Max(0, Math.Min(value, Props.MaxStored));
            var avData = Props.AvailableItems.Where(d => d.ArchiteCount < value).ToList();
            _maxUnloadableContainer = avData.Count > 0 ? avData.MaxBy(d => d.ArchiteCount) : null;
        }
    }

    public float StoredPercent => Stored / Props.MaxStored;

    private float FreeSpace => Props.MaxStored - Stored;

    public override void Initialize(CompProperties p)
    {
        base.Initialize(p);
        _minContainer = Props.AvailableItems.MinBy(c => c.ArchiteCount);
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref _stored, "Stored");
        Scribe_Values.Look(ref _allowAutoRefuel, "AllowAutoRefuel");
        Stored = _stored;
    }

    public override void PostDraw()
    {
        if (!_allowAutoRefuel)
            Parent.Map.overlayDrawer.DrawOverlay(Parent, OverlayTypes.ForbiddenRefuel);
        if (Stored <= 0)
            Parent.Map.overlayDrawer.DrawOverlay(Parent, OverlayTypes.OutOfFuel);
    }

    public IEnumerable<Thing> GetAvailableFuels(Map map)
    {
        return Props.AvailableItems.Where(f => f.ArchiteCount <= FreeSpace).SelectMany(f => map.listerThings.ThingsOfDef(f.ThingDef)).ToList();
    }

    public bool IsThingAllowedAsFuel(Thing thing)
    {
        return Props.AvailableItems.Any(f => f.ThingDef == thing.def && f.ArchiteCount <= FreeSpace);
    }

    public bool CanStoreMore()
    {
        return Power.PowerOn && FreeSpace >= _minContainer.ArchiteCount;
    }

    public int CountToFullyRefuel(Thing thing)
    {
        var avItem = Props.AvailableItems.Find(av => av.ThingDef == thing.def);
        if (avItem == null) return 0;
        return (int)Math.Floor(FreeSpace / avItem.ArchiteCount);
    }

    public void InsertFuel(Thing fuel)
    {
        var data = Props.AvailableItems.First(i => i.ThingDef == fuel.def);
        Stored += data.ArchiteCount * fuel.stackCount;
    }

    public bool Consume(ref float amount)
    {
        // todo! balance tha containers
        if (Stored >= amount)
        {
            Stored -= amount;
            amount = 0;
            return true;
        }

        amount -= Math.Min(Stored, amount);
        Stored = 0;
        return false;
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Archite stored: {Stored:0.##}/{Props.MaxStored:0.##} µU");
        return sb.TrimEnd().ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "Unload",
            Disabled = _maxUnloadableContainer is null,
            disabledReason = "Not enought archite to fill any container",
            action = () =>
            {
                var thing = _maxUnloadableContainer.ThingDef.MadeFromStuff //
                    ? ThingMaker.MakeThing(_maxUnloadableContainer.ThingDef, GenStuff.DefaultStuffFor(_maxUnloadableContainer.ThingDef))
                    : ThingMaker.MakeThing(_maxUnloadableContainer.ThingDef);
                GenDrop.TryDropSpawn(thing, Parent.InteractionCell, Parent.Map, ThingPlaceMode.Near, out var dropped);
                if (dropped != null) Stored -= _maxUnloadableContainer.ArchiteCount;

                // SoundDefOf.DropPod_Open.PlayOneShot(parent);
            }
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "-10%", action = () => Stored -= Props.MaxStored * 0.1f };
            yield return new Command_Action { defaultLabel = "+10%", action = () => Stored += Props.MaxStored * 0.1f };
            yield return new Command_Action { defaultLabel = "Full", action = () => Stored = Props.MaxStored };
        }

        yield return new Command_Toggle
        {
            defaultLabel = "CommandToggleAllowAutoRefuel".Translate(),
            defaultDesc = "CommandToggleAllowAutoRefuelDesc".Translate(),
            hotKey = KeyBindingDefOf.Command_ItemForbid,
            icon = _allowAutoRefuel ? TexCommand.ForbidOff : (Texture)TexCommand.ForbidOn,
            isActive = () => _allowAutoRefuel,
            toggleAction = () => _allowAutoRefuel = !_allowAutoRefuel
        };

        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;
    }
}

public class ItemData
{
    public int ArchiteCount;
    public ThingDef ThingDef;
}
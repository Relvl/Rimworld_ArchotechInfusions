using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ArchotechInfusions.building.proto;
using ArchotechInfusions.comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.building;

[StaticConstructorOnStartup]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ArchInf_Container_Building : AGridBuilding
{
    private static readonly Vector3 Offset = Vector3.up * 0.1f + Vector3.back * 0.35f;
    private static readonly Vector2 FuelBarSize = new(0.75f, 0.2f);
    public static readonly Material ContainerFuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));
    public static readonly Material ContainerFuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private bool _allowAutoRefuel = true;

    private Comp_ArchiteContainer _comp;

    private CompProps_ArchiteContainer.ItemData _maxUnloadableContainer;
    private CompProps_ArchiteContainer.ItemData _minContainer;

    private float _stored;

    private Comp_ArchiteContainer Comp => _comp ??= GetComp<Comp_ArchiteContainer>();

    public float Stored
    {
        get => _stored;
        private set
        {
            _stored = Math.Max(0, Math.Min(value, Comp.Props.MaxStored));
            var avData = Comp.Props.AvailableItems.Where(d => d.ArchiteCount < value).ToList();
            _maxUnloadableContainer = avData.Count > 0 ? avData.MaxBy(d => d.ArchiteCount) : null;
        }
    }

    public float StoredPercent => Stored / Comp.Props.MaxStored;

    private float FreeSpace => Comp.Props.MaxStored - Stored;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _stored, "Stored");
        Scribe_Values.Look(ref _allowAutoRefuel, "AllowAutoRefuel");
        Stored = _stored;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        _minContainer = Comp.Props.AvailableItems.MinBy(c => c.ArchiteCount);
        base.SpawnSetup(map, respawningAfterLoad);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = DrawPos + Offset,
            size = FuelBarSize,
            fillPercent = StoredPercent,
            filledMat = ContainerFuelBarFilledMat,
            unfilledMat = ContainerFuelBarUnfilledMat,
            margin = 0.15f,
            rotation = Rotation.CopyAndRotate(RotationDirection.Clockwise)
        });

        if (!_allowAutoRefuel)
            Map.overlayDrawer.DrawOverlay(this, OverlayTypes.ForbiddenRefuel);
        if (Stored <= 0)
            Map.overlayDrawer.DrawOverlay(this, OverlayTypes.OutOfFuel);
    }

    protected override void FillInspectStringExtra(StringBuilder sb)
    {
        sb.AppendLine($"Archite stored: {Stored:0.##}/{Comp.Props.MaxStored:0.##} µU");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

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
                GenDrop.TryDropSpawn(thing, InteractionCell, Map, ThingPlaceMode.Near, out var dropped);
                if (dropped != null) Stored -= _maxUnloadableContainer.ArchiteCount;

                // SoundDefOf.DropPod_Open.PlayOneShot(parent);
            }
        };

        yield return new Command_Toggle
        {
            defaultLabel = "CommandToggleAllowAutoRefuel".Translate(),
            defaultDesc = "CommandToggleAllowAutoRefuelDesc".Translate(),
            hotKey = KeyBindingDefOf.Command_ItemForbid,
            icon = _allowAutoRefuel ? TexCommand.ForbidOff : (Texture)TexCommand.ForbidOn,
            isActive = () => _allowAutoRefuel,
            toggleAction = () => _allowAutoRefuel = !_allowAutoRefuel
        };

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action { defaultLabel = "Empty", action = () => Stored = 0 };
            yield return new Command_Action { defaultLabel = "-10%", action = () => Stored -= Comp.Props.MaxStored * 0.1f };
            yield return new Command_Action { defaultLabel = "+10%", action = () => Stored += Comp.Props.MaxStored * 0.1f };
            yield return new Command_Action { defaultLabel = "Full", action = () => Stored = Comp.Props.MaxStored };
        }
    }

    public IEnumerable<Thing> GetAvailableFuels(Map map)
    {
        return Comp.Props.AvailableItems.Where(f => f.ArchiteCount <= FreeSpace).SelectMany(f => map.listerThings.ThingsOfDef(f.ThingDef)).ToList();
    }

    public bool IsThingAllowedAsFuel(Thing thing)
    {
        return Comp.Props.AvailableItems.Any(f => f.ThingDef == thing.def && f.ArchiteCount <= FreeSpace);
    }

    public bool CanStoreMore()
    {
        return Grid.PowerOn && FreeSpace >= _minContainer.ArchiteCount;
    }

    public int CountToFullyRefuel(Thing thing)
    {
        var avItem = Comp.Props.AvailableItems.Find(av => av.ThingDef == thing.def);
        if (avItem == null) return 0;
        return (int)Math.Floor(FreeSpace / avItem.ArchiteCount);
    }

    public void InsertFuel(Thing fuel)
    {
        var data = Comp.Props.AvailableItems.First(i => i.ThingDef == fuel.def);
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
}
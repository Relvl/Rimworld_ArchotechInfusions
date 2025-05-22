using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.comps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.ui.print;

public class ThingSelectorWindow : Window
{
    private readonly Comp_Printer _comp;
    private readonly Pawn _pawn;
    private readonly PrintWindowSelector _selector;
    private float listHeight;
    private Vector2 scrollPosition = Vector2.zero;

    public ThingSelectorWindow(PrintWindowSelector selector, Pawn pawn, Comp_Printer comp)
    {
        _selector = selector;
        _pawn = pawn;
        _comp = comp;

        draggable = true;
        doCloseX = true;
        resizeable = true;
        forcePause = true;
    }

    public override void PostClose()
    {
        _selector.OnThingSelectorWindowClosed();
        base.PostClose();
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = Color.cyan;

        Widgets.Label(inRect, "JAI.Printer.SelectThing.Title".Translate());
        inRect.yMin += 24;

        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        var y = 0f;
        var isScrollView = listHeight > inRect.height;
        var viewRect = new Rect(0, 0, inRect.width - (isScrollView ? 16 : 0), listHeight);
        Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

        var apparel = _pawn.apparel.WornApparel.ToList();
        if (apparel.Count > 0)
        {
            Widgets.ListSeparator(ref y, viewRect.width, "Apparel".Translate());
            foreach (var thing in apparel)
                DrawThingRow(ref y, viewRect.width, thing);
        }

        var equipment = _pawn.equipment.AllEquipmentListForReading.ToList();
        if (equipment.Count > 0)
        {
            Widgets.ListSeparator(ref y, viewRect.width, "Equipment".Translate());
            foreach (var thing in equipment)
                DrawThingRow(ref y, viewRect.width, thing);
        }

        var inventory = _pawn.inventory.innerContainer.ToList();
        if (inventory.Count > 0)
        {
            Widgets.ListSeparator(ref y, viewRect.width, "Inventory".Translate());
            foreach (var thing in inventory)
                DrawThingRow(ref y, viewRect.width, thing);
        }

        listHeight = y + 20;
        Widgets.EndScrollView();
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    private void DrawThingRow(ref float y, float width, Thing thing)
    {
        if (!thing.TryGetInfusedComp(out var comp))
            return;

        var rowRect = new Rect(0.0f, y, width, 28f);
        var buttonLabel = "JAI.Printer.SelectThing.Select".Translate();
        var buttonWidth = Text.CalcSize(buttonLabel).x + 16;
        var buttonRect = new Rect(rowRect.xMax - buttonWidth, y + 1, buttonWidth, rowRect.height - 2);
        var thingRect = rowRect with { xMax = rowRect.xMax - buttonWidth - 4 };

        thing.Draw(thingRect, true, " " + "JAI.instruction.integrity.value".Translate(comp.Integrity), comp);

        Text.Anchor = TextAnchor.MiddleCenter;
        if (Widgets.ButtonText(buttonRect, buttonLabel))
        {
            SoundDefOf.Crunch.PlayOneShotOnCamera();
            _selector.OnThingSelected(_pawn, thing);
        }

        y += 28f;
    }
}
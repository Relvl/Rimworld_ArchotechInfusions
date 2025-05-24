using System;
using ArchotechInfusions.building.proto;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.graphic;

public class GridInfoWindow : Window
{
    private static readonly string ColorHtmlGreen = ColorUtility.ToHtmlStringRGBA(Color.green);

    private readonly AGridBuilding _member;

    private Vector2 _lastFrameSize;
    private Vector2 _scroll = Vector2.zero;

    public GridInfoWindow(AGridBuilding member)
    {
        doCloseX = true;
        resizeable = true;
        draggable = true;
        _member = member;
    }

    public override void PreOpen()
    {
        base.PreOpen();
        GridMapComponent.GridToDebug = _member.Grid;
        _member?.Map?.mapDrawer?.RegenerateEverythingNow();
    }

    public override void PreClose()
    {
        base.PreClose();
        GridMapComponent.GridToDebug = null;
        _member?.Map?.mapDrawer?.RegenerateEverythingNow();
    }

    public override void DoWindowContents(Rect inRect)
    {
        var inner = new Rect(0, 0, Math.Max(_lastFrameSize.x, inRect.width), Math.Max(_lastFrameSize.y, inRect.height));
        Widgets.BeginScrollView(inRect, ref _scroll, inner);

        Text.Font = GameFont.Medium;
        GUI.color = Color.green;
        Widgets.Label(inner, "Grid info");
        inner.yMin += 24;

        var grid = _member.Grid;
        RenderLine(ref inner, $"Grid ID: <color={ColorHtmlGreen}>{grid.Guid}</color>");
        RenderLine(ref inner, $"Grid members: {grid.Members.Count}");

        foreach (var member in grid.Members)
            RenderLine(ref inner, $"\t+ {member.Label} [{member.ThingID}] pos:({member.Position})");

        // RenderLine(ref inner, $"");
        Widgets.EndScrollView();

        _lastFrameSize.x = inner.xMin;
        _lastFrameSize.y = inner.yMin;
    }

    private void RenderLine(ref Rect inRect, string text)
    {
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        Widgets.Label(inRect, text);
        inRect.yMin += 20;
    }
}
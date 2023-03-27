using System;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid.graphic;

public class GridInfoWindow : Window
{
    private static string ColorHtmlGreen = ColorUtility.ToHtmlStringRGBA(Color.green);

    private readonly GridMemberComp _member;

    private Vector2 _lastFrameSize;
    private Vector2 _scroll = Vector2.zero;

    public GridInfoWindow(GridMemberComp member)
    {
        doCloseX = true;
        resizeable = true;
        draggable = true;
        _member = member;
    }

    public override void PreOpen()
    {
        base.PreOpen();
        GridMapComponent.DebudGrid = _member.Grid;
        _member.parent.Map.mapDrawer.RegenerateEverythingNow();
    }

    public override void PreClose()
    {
        base.PreClose();
        GridMapComponent.DebudGrid = null;
        _member.parent.Map.mapDrawer.RegenerateEverythingNow();
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
        RenderLine(ref inner, $"Grid type: <color={ColorHtmlGreen}>{grid.GridType}</color>");
        RenderLine(ref inner, $"Grid members: {grid.Members.Count}");

        foreach (var member in grid.Members)
        {
            RenderLine(ref inner, $"\t+ {member.parent.Label} [{member.parent.ThingID}] pos:({member.parent.Position})");
        }

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
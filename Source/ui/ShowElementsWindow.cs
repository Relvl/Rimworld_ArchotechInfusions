using System;
using System.Linq;
using System.Text;
using ArchotechInfusions.statcollectors;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.ui;

public class ShowElementsWindow : Window
{
    private const int MinCellWidth = 220;
    private const int CellHeight = 45;
    private const int CellMargin = 10;

    private Vector2 _scroll;
    private float _lastFrameHeight;

    public ShowElementsWindow()
    {
        draggable = true;
        doCloseX = true;
        resizeable = true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Tiny;
        Text.WordWrap = false;

        var innerRect = new Rect(0, 0, inRect.width - 16, _lastFrameHeight);
        var columnCount = Math.Max((int)Math.Floor(innerRect.width / MinCellWidth), 1);
        var columnWidth = innerRect.width / columnCount;

        Widgets.BeginScrollView(inRect, ref _scroll, innerRect);
        var idx = 0;
        foreach (var element in StatCollector.StatCache.OrderBy(e => e.Order()))
        {
            var colIdx = idx % columnCount;
            var rowIdx = idx / columnCount;
            var cellRect = new Rect((columnWidth + CellMargin) * colIdx, inRect.y + (CellHeight + CellMargin) * rowIdx, columnWidth, CellHeight);
            GUI.DrawTexture(cellRect, TexUI.HighlightTex);
            TooltipHandler.TipRegion(cellRect, $"defName: {element.StatDef.defName}");

            cellRect = cellRect.ContractedBy(2);
            Widgets.Label(cellRect, $"<color=#00FF00>{element.StatDef.LabelCap}</color>");
            cellRect.yMin += 13;

            var sb = new StringBuilder();
            sb.Append(element.Modifier.TechLevel);
            if (element.MulUsed) sb.Append($" / Mul: {element.Modifier.Mul.x:0.###}..{element.Modifier.Mul.y:0.###}");
            if (element.AddUsed) sb.Append($" / Add: {element.Modifier.Add.x:0.###}..{element.Modifier.Add.y:0.###}");
            Widgets.Label(cellRect, sb.ToString());
            cellRect.yMin += 11;

            Widgets.Label(cellRect, $"Weight: {element.Order():0.##}");

            idx++;
        }

        _lastFrameHeight = idx / columnCount * (CellHeight + CellMargin);
        Widgets.EndScrollView();

        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
    }
}
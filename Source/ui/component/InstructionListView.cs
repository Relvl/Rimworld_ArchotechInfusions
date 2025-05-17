using System;
using System.Collections.Generic;
using ArchotechInfusions.statcollectors;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.ui.component;

public static class InstructionListView
{
    public const float Margin = 10f;

    private static Vector2 _scrollPosition = Vector2.zero;
    private static float _lastFrameHeight;

    public static void Draw(this IEnumerable<Instruction> instructions, Rect inRect, Func<Instruction, bool> isActiveFunc = null, Func<Instruction, InstructionView.ButtonData[]> buttonsFunc = null)
    {
        Text.Font = GameFont.Tiny;
        Text.WordWrap = false;
        GUI.color = Color.white;

        var lastFrameScrollsVisible = _lastFrameHeight > inRect.height;
        var viewRect = new Rect(0, 0, inRect.width - (lastFrameScrollsVisible ? 16 : 0), _lastFrameHeight);

        var columnCount = Math.Max((int)Math.Floor((viewRect.width + Margin) / (InstructionView.MinWidth + Margin)), 1);
        var columnWidth = (viewRect.width - (columnCount - 1) * Margin) / columnCount;

        Widgets.BeginScrollView(inRect, ref _scrollPosition, viewRect);

        var cellRect = new Rect(0f, 0f, columnWidth, InstructionView.Height);
        var lastRealY = 0f;

        foreach (var instruction in instructions)
        {
            var active = isActiveFunc?.Invoke(instruction) ?? false;
            var buttons = buttonsFunc?.Invoke(instruction) ?? [];
            instruction.Draw(cellRect, active, buttons);

            lastRealY = cellRect.yMax;
            cellRect.x += columnWidth + Margin;
            if (cellRect.xMax > viewRect.xMax)
            {
                cellRect.x = 0f;
                cellRect.y += InstructionView.Height + Margin;
            }
        }

        _lastFrameHeight = lastRealY;
        Widgets.EndScrollView();

        Text.Font = GameFont.Small;
        Text.WordWrap = true;
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}
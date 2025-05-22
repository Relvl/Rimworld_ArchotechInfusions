using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.instructions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.ui.component;

public static class InstructionView
{
    private static readonly StringBuilder Builder = new();
    public const float Height = 45f;
    public const float MinWidth = 220f;
    private static readonly Color MenuSectionBgBorderColor = new ColorInt(135, 135, 135).ToColor;
    private static readonly Color HoverColor = new ColorInt(205, 197, 10).ToColor.ToTransparent(0.5f);

    public static void Draw(this AInstruction instruction, Rect cellRect, bool active = false, params ButtonData[] buttons)
    {
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;

        var hover = Mouse.IsOver(cellRect);
        var hoverRect = cellRect;

        GUI.color = active ? Color.yellow : instruction.BgColor;
        GUI.DrawTexture(cellRect, TexUI.HighlightTex);
        GUI.color = active ? Color.yellow : hover ? HoverColor : MenuSectionBgBorderColor;
        Widgets.DrawBox(cellRect);

        GUI.color = Color.white;

        cellRect = cellRect.ContractedBy(4);
        var buttonsX = cellRect.xMax - Widgets.CloseButtonSize;
        var buttonsY = cellRect.y;

        foreach (var line in CellLines(instruction))
        {
            Text.WordWrap = false;
            Widgets.Label(cellRect, line);
            cellRect.yMin += Text.LineHeight * 0.6f;
        }

        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            var buttonRect = new Rect(buttonsX, buttonsY + (Widgets.CloseButtonSize + 2) * i, Widgets.CloseButtonSize, Widgets.CloseButtonSize);
            if (Widgets.ButtonImage(buttonRect, button.Texture, true, button.Tooltip))
            {
                SoundDefOf.Crunch.PlayOneShotOnCamera();
                button.OnClick?.Invoke(instruction);
            }
        }

        if (hover)
        {
            Builder.Clear();
            instruction.RenderTooltip(Builder);
            var tooltip = Builder.ToString();
            if (!tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(hoverRect, tooltip);
        }
    }

    private static IEnumerable<string> CellLines(AInstruction instruction)
    {
        Builder.Clear();
        instruction.RenderLabel(Builder);
        instruction.RenderValue(Builder);
        yield return Builder.ToString();

        Builder.Clear();
        instruction.RenderComplexity(Builder);
        yield return Builder.ToString();

        Builder.Clear();
        instruction.RenderExtraLine(Builder);

        yield return Builder.ToString();
    }

    public class ButtonData
    {
        public string Tooltip;
        public Action<AInstruction> OnClick;
        public Texture2D Texture;
    }
}
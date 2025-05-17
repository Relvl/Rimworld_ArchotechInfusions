using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.statcollectors;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.ui.component;

public static class InstructionView
{
    public const float Height = 45f;
    public const float MinWidth = 220f;
    private static readonly Color MenuSectionBgBorderColor = new ColorInt(135, 135, 135).ToColor;

    public static void Draw(this Instruction instruction, Rect cellRect, bool active = false, params ButtonData[] buttons)
    {
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;

        if (active) GUI.color = Color.yellow;
        GUI.DrawTexture(cellRect, TexUI.HighlightTex);
        GUI.color = active ? Color.yellow : MenuSectionBgBorderColor;
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
    }

    private static IEnumerable<string> CellLines(Instruction instruction)
    {
        var sb = new StringBuilder();

        sb.Clear();
        sb.Append("<color=#00FF00>").Append(instruction.Def.LabelCap).Append("</color> ");
        instruction.FillValueString(sb);
        yield return sb.ToString();

        sb.Clear();
        sb.Append("Complexity: ").Append(instruction.Complexity.ToString("0.##"));
        yield return sb.ToString();

        sb.Clear();
        sb.Append("B: ").Append(instruction.Def.defaultBaseValue.ToString("0.##")).Append(", ");
        sb.Append("R: ").Append(instruction.Def.Worker.ValueToString(instruction.Value, true)).Append(", ");
        yield return sb.ToString();
    }

    public class ButtonData
    {
        public string Tooltip;
        public Action<Instruction> OnClick;
        public Texture2D Texture;
    }
}
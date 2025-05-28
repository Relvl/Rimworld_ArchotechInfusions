using System;
using System.Collections.Generic;
using System.Text;
using ArchotechInfusions.injected;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions;

public static class Extensions
{
    public static StringBuilder TrimEnd(this StringBuilder sb)
    {
        if (sb == null || sb.Length == 0) return sb;
        var i = sb.Length - 1;
        for (; i >= 0; i--)
            if (!char.IsWhiteSpace(sb[i]))
                break;
        if (i < sb.Length - 1)
            sb.Length = i + 1;
        return sb;
    }

    public static Rot4 CopyAndRotate(this Rot4 input, RotationDirection direction)
    {
        var copy = input;
        copy.Rotate(direction);
        return copy;
    }

    public static void Draw(this Thing thing, Rect rowRect, bool highlight = false, string extraText = "", InstructionsComps comp = null)
    {
        GUI.color = Color.white;
        Text.Font = GameFont.Small;

        var isOver = Mouse.IsOver(rowRect);
        if (isOver && highlight)
        {
            GUI.color = ITab_Pawn_Gear.HighlightColor;
            GUI.DrawTexture(rowRect, TexUI.HighlightTex);
        }

        Widgets.InfoCardButton(0, rowRect.yMin + 2, thing);
        rowRect.xMin += 28;

        if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
        {
            Widgets.ThingIcon(new Rect(rowRect.xMin, rowRect.yMin, 28f, 28f), thing);
            rowRect.xMin += 32;
        }

        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = ITab_Pawn_Gear.ThingLabelColor;

        var nameRect = new Rect(rowRect.xMin, rowRect.yMin, rowRect.width - 36f, rowRect.height);

        // todo crop rect by total text width

        Text.WordWrap = false;
        var text = (thing.LabelCap + extraText).Truncate(nameRect.width);
        Widgets.Label(nameRect, text);
        Text.WordWrap = true;

        Text.Anchor = TextAnchor.MiddleLeft;
        if (isOver)
        {
            var sb = new StringBuilder();
            sb.Append(thing.LabelNoParenthesisCap.AsTipTitle()).Append(GenLabel.LabelExtras(thing, true, true));
            sb.AppendLine().AppendLine();
            sb.AppendLine(thing.DescriptionDetailed);

            if (thing.def.useHitPoints) sb.Append(thing.HitPoints).Append(" / ").Append(thing.MaxHitPoints).AppendLine();

            if (comp is not null)
            {
                sb.AppendLine();
                sb.AppendLine("JAI.instruction.integrity.value".Translate(comp.Integrity));
                foreach (var instruction in comp.Instructions)
                {
                    sb.Append(instruction.Label).Append(": ");
                    instruction.RenderValue(sb);
                    sb.AppendLine();
                }
            }

            TooltipHandler.TipRegion(rowRect, sb.ToString());
        }
    }

    public static TValue ComputeIfAbsent<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> func)
    {
        if (dictionary.TryGetValue(key, out var result)) return result;
        result = func(key);
        dictionary.Add(key, result);
        return result;
    }

    public static bool TryGetInfusedComp(this Thing thing, out InstructionsComps comp)
    {
        if (thing is null)
        {
            comp = null;
            return false;
        }

        comp = thing.TryGetComp<InstructionsComps>();
        return comp is not null;
    }
}
using System;
using System.Linq;
using System.Text;
using ArchotechInfusions.comps;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.ui;

public class CompDatabaseWindow : Window
{
    private const int MinCellWidth = 220;
    private const int CellHeight = 45;
    private const int CellMargin = 10;

    private readonly Comp_Database _comp;
    private float _lastFrameHeight;
    private Vector2 _scroll;

    public CompDatabaseWindow(Comp_Database comp)
    {
        _comp = comp;
        draggable = true;
        resizeable = true;
        doCloseX = true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        GUI.color = Color.cyan;
        Widgets.Label(inRect, _comp.parent.LabelCap);

        inRect.yMin += 24;

        Text.Font = GameFont.Tiny;
        Text.WordWrap = false;
        GUI.color = Color.white;

        var innerRect = new Rect(0, 0, inRect.width - 16, _lastFrameHeight);
        var columnCount = Math.Max((int)Math.Floor(innerRect.width / MinCellWidth), 1);
        var columnWidth = innerRect.width / columnCount;

        Widgets.BeginScrollView(inRect, ref _scroll, innerRect);
        var idx = 0;

        foreach (var modifier in _comp.Modifiers.ToList())
        {
            var colIdx = idx % columnCount;
            var rowIdx = idx / columnCount;
            var cellRect = new Rect((columnWidth + CellMargin) * colIdx, inRect.y + (CellHeight + CellMargin) * rowIdx, columnWidth, CellHeight);
            GUI.DrawTexture(cellRect, TexUI.HighlightTex);

            cellRect = cellRect.ContractedBy(2);
            var closeCell = new Rect(cellRect.xMax - Widgets.CloseButtonSize, cellRect.y, Widgets.CloseButtonSize, Widgets.CloseButtonSize);

            var sb = new StringBuilder();
            sb.Append("<color=#00FF00>").Append(modifier.Def.LabelCap).Append("</color> ");
            modifier.FillValueString(sb);
            Widgets.Label(cellRect, sb.ToString());
            cellRect.yMin += 13;

            sb.Clear();
            sb.Append("Complexity: ").Append(modifier.Complexity.ToString("0.##"));
            Widgets.Label(cellRect, sb.ToString());
            cellRect.yMin += 13;

            sb.Clear();
            sb.Append("B: ").Append(modifier.Def.defaultBaseValue.ToString("0.##")).Append(", ");
            sb.Append("R: ").Append(modifier.Def.Worker.ValueToString(modifier.Value, true)).Append(", ");
            Widgets.Label(cellRect, sb.ToString());
            cellRect.yMin += 13;

            if (Widgets.ButtonImage(closeCell, TexButton.CloseXSmall))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    _comp.RemoveModifier(modifier);
                else
                    Find.WindowStack.Add(
                        Dialog_MessageBox.CreateConfirmation(
                            "ArchInf.Message.RemoveInstruction".Translate(),
                            () => _comp.RemoveModifier(modifier),
                            true,
                            "ArchInf.Message.RemoveInstruction.Title".Translate()
                        )
                    );
            }

            idx++;
        }

        _lastFrameHeight = (float)Math.Floor(idx / (double)columnCount) * (CellHeight + CellMargin);
        Widgets.EndScrollView();

        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
    }
}
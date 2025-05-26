using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.ui;

public class StatListWindow : Window
{
    private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;
    private Vector2 _scrollPosition;
    private string _searchText = string.Empty;

    public StatListWindow()
    {
        doCloseButton = false;
        doCloseX = true;
        forcePause = true;
        draggable = true;
    }

    public override Vector2 InitialSize => new(750f, 600f);

    private static IEnumerable<StatDef> StatDefs => DefDatabase<StatDef>.AllDefs.OrderBy(s => s.defName);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 35f), "Stat List");

        Text.Font = GameFont.Small;

        var searchWidth = inRect.width - 100f;
        var searchRect = new Rect(inRect.x, inRect.y + 35f, searchWidth, 25f);
        _searchText = Widgets.TextField(searchRect, _searchText);

        var bulkCopyRect = new Rect(searchRect.xMax + 5f, searchRect.y, 90f, 25f);
        if (Widgets.ButtonText(bulkCopyRect, "Copy All"))
        {
            var defsText = string.Join("\n", StatDefs
                .Where(s => (s.defName?.ToLower() ?? "").Contains(_searchText.ToLower())
                            || (s.label?.ToLower() ?? "").Contains(_searchText.ToLower())
                            || (s.description?.ToLower() ?? "").Contains(_searchText.ToLower()))
                .Select(s => $"{s.defName} ({s.category?.defName})"));

            GUIUtility.systemCopyBuffer = defsText;
            Messages.Message("Copied all filtered defNames to clipboard", MessageTypeDefOf.SilentInput, false);
        }

        const float headerHeight = 65f;
        var outRect = new Rect(inRect.x, inRect.y + headerHeight, inRect.width, inRect.height - headerHeight);

        var filteredStats = StatDefs
            .Where(s => (s.defName?.ToLower() ?? "").Contains(_searchText.ToLower())
                        || (s.label?.ToLower() ?? "").Contains(_searchText.ToLower())
                        || (s.description?.ToLower() ?? "").Contains(_searchText.ToLower()))
            .ToList();

        var viewRect = new Rect(0, 0, outRect.width - 16f, filteredStats.Count * 30f);

        Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

        var y = 0f;
        foreach (var stat in filteredStats)
        {
            var rowRect = new Rect(0, y, viewRect.width, 30f);

            // Info button with tooltip
            var infoRect = new Rect(rowRect.x, rowRect.y, 24f, 24f);
            TooltipHandler.TipRegion(infoRect, (stat.description ?? "No description") + $"\n\n{stat.category?.defName ?? "No category"}");
            Widgets.InfoCardButton(infoRect.x, infoRect.y, stat);

            // Copy button on the right
            const float copyButtonWidth = 60f;
            var copyButtonRect = new Rect(rowRect.xMax - copyButtonWidth, rowRect.y, copyButtonWidth, 24f);
            if (Widgets.ButtonText(copyButtonRect, "Copy"))
            {
                GUIUtility.systemCopyBuffer = $"{stat.defName} (${stat.category?.defName})";
                Messages.Message($"Copied '{stat.defName}' to clipboard", MessageTypeDefOf.SilentInput, false);
            }

            // Measure defName width, add small padding, and align to right
            var defNameText = stat.defName;
            var defNameTextWidth = Text.CalcSize(defNameText).x + 4f; // small extra space to avoid wrapping
            var defNameRect = new Rect(copyButtonRect.x - defNameTextWidth - 10f, rowRect.y, defNameTextWidth, 30f);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(defNameRect, defNameText);
            Text.Anchor = TextAnchor.UpperLeft;

            // Label at the beginning of the row
            var labelRect = new Rect(infoRect.xMax + 5f, rowRect.y, defNameRect.x - infoRect.xMax - 10f, 30f);
            var capitalizedLabel = _textInfo.ToTitleCase(stat.label ?? "");
            Widgets.Label(labelRect, capitalizedLabel);

            y += 30f;
        }

        Widgets.EndScrollView();
    }
}
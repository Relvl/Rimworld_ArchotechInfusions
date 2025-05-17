using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchotechInfusions.comps;
using ArchotechInfusions.statcollectors;
using ArchotechInfusions.ui.component;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.ui.print;

public class PrintWindow : Window
{
    private readonly InstructionView.ButtonData _dequeueButton;
    private readonly InstructionView.ButtonData _enqueueButton;
    private readonly List<Instruction> _instructionsToApply = [];

    private readonly Pawn _pawn;
    private readonly Comp_Printer _printer;
    private readonly PrintWindowSelector _selector;
    private readonly Thing _thing;

    public override Vector2 InitialSize => new(800, 600);

    public PrintWindow(PrintWindowSelector selector, Pawn pawn, Comp_Printer printer, Thing thing)
    {
        _selector = selector;
        _pawn = pawn;
        _printer = printer;
        _thing = thing;

        draggable = true;
        doCloseX = true;
        resizeable = true;
        forcePause = true;

        _dequeueButton = new InstructionView.ButtonData { Tooltip = "Do not print this instruction", Texture = TexButton.CloseXSmall, OnClick = OnDequeue };
        _enqueueButton = new InstructionView.ButtonData { Tooltip = "Print this instruction", Texture = TexButton.Play, OnClick = OnEnqueue };
    }

    public override void PostClose()
    {
        _selector.OnPrintWindowClosed();
        _instructionsToApply.Clear();
        base.PostClose();
    }

    public void OnEnqueue(Instruction instruction)
    {
        _instructionsToApply.Add(instruction);
    }

    public void OnDequeue(Instruction instruction)
    {
        _instructionsToApply.Remove(instruction);
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        var titleRect = new Rect(0, 0, inRect.width, Text.LineHeight);

        Text.Font = GameFont.Small;
        var thingRect = new Rect(0, titleRect.yMax + StandardMargin / 2, inRect.width, 28);

        var buttonsRect = new Rect(0, inRect.yMax - Widgets.BackButtonHeight, inRect.width, Widgets.BackButtonHeight);

        Text.Font = GameFont.Small;
        var totalsHeight = Text.LineHeight * 1.2f;
        var totalsRect = new Rect(0, buttonsRect.yMin - totalsHeight - StandardMargin / 2, inRect.width, totalsHeight);

        inRect.yMin = thingRect.yMax + StandardMargin;
        inRect.yMax = totalsRect.yMin - StandardMargin;

        Text.Font = GameFont.Medium;
        GUI.color = Color.cyan;
        Widgets.Label(titleRect, "JAI.Printer.Print.Title".Translate());
        _thing.Draw(thingRect, false, $", HP: {_thing.HitPoints}/{_thing.MaxHitPoints}, Integrity: {120f:0.00}");

        var instructions = _printer.Member.Grid.GetComps<Comp_Database>().SelectMany(database => database.Modifiers);
        instructions.Draw(inRect, i => _instructionsToApply.Contains(i), i => _instructionsToApply.Contains(i) ? [_dequeueButton] : [_enqueueButton]);

        DrawTotals(totalsRect);

        DrawButtons(buttonsRect);

        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
    }

    private void DrawButtons(Rect buttonsRect)
    {
        if (Widgets.ButtonText(buttonsRect with { width = 100 }, "Back".Translate(), overrideTextAnchor: TextAnchor.MiddleCenter))
        {
            _selector.ForceClose();
        }

        var confirmButtonRect = buttonsRect with { xMin = buttonsRect.width - 100 };
        if (Widgets.ButtonText(confirmButtonRect, "JAI.Printer.Print.Confirm".Translate(), true, true, _instructionsToApply.Count > 0, TextAnchor.MiddleCenter))
        {
            SoundDefOf.Crunch.PlayOneShotOnCamera();
            foreach (var instruction in _instructionsToApply)
                _printer.EnqueueInstruction(instruction, _thing);
            _selector.ForceClose();
        }
    }

    private void DrawTotals(Rect inRect)
    {
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        var totalComplexity = 0f;
        var totalArchite = 0f;
        foreach (var instruction in _instructionsToApply)
        {
            totalComplexity += instruction.Complexity;
            totalArchite += _printer.Props.PrintArchiteCost;
        }

        Text.Font = GameFont.Small;
        Widgets.DrawMenuSection(inRect);
        var textRect = inRect.ContractedBy(4);

        var sb = new StringBuilder();
        sb.Append("Instructions: ").Append(_instructionsToApply.Count).Append(". Total complexity: ").Append(totalComplexity.ToString("0.00")).Append(". Total archite needed: ").Append(totalArchite).AppendLine(".");
        Widgets.Label(textRect, sb.ToString());
    }
}
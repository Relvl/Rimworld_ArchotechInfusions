using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchotechInfusions.comps;
using ArchotechInfusions.injected;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui.component;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.ui.print;

public class PrintWindow : Window
{
    private readonly Comp_ArchInfused _comp;

    private readonly InstructionView.ButtonData _dequeueButton;
    private readonly InstructionView.ButtonData _enqueueButton;

    private readonly Comp_Printer _printer;
    private readonly PrintWindowSelector _selector;
    private readonly Thing _thing;
    private float _architeLeft;

    private float _architeNeeded;
    private AInstruction _instruction;

    public PrintWindow(PrintWindowSelector selector, Comp_Printer printer, Thing thing)
    {
        _selector = selector;
        _printer = printer;
        _thing = thing;
        thing.TryGetInfusedComp(out _comp);

        draggable = true;
        doCloseX = true;
        resizeable = true;
        forcePause = true;

        _dequeueButton = new InstructionView.ButtonData { Tooltip = "Cancel".Translate(), Texture = TexButton.CloseXSmall, OnClick = OnDequeue };
        _enqueueButton = new InstructionView.ButtonData { Tooltip = "JAI.Printer.Print.Title".Translate(), Texture = TexButton.Play, OnClick = OnEnqueue };
    }

    public override Vector2 InitialSize => new(800, 600);

    public override void PostClose()
    {
        _selector.OnPrintWindowClosed();
        _instruction = null;
        base.PostClose();
    }

    private void OnEnqueue(AInstruction instruction)
    {
        _instruction = instruction;
    }

    private void OnDequeue(AInstruction instruction)
    {
        _instruction = null;
    }

    public override void WindowUpdate()
    {
        base.WindowUpdate();

        if (_instruction is not null)
        {
            var archite = (float)_printer.Props.PrintArchiteCost;
            _instruction.ModifyArchiteConsumed(ref archite);
            _architeLeft = _printer.Grid.GetTotalArchite();
            _architeNeeded = archite;
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        var titleRect = new Rect(0, 0, inRect.width, Text.LineHeight);

        Text.Font = GameFont.Small;
        var thingRect = new Rect(0, titleRect.yMax + StandardMargin / 2, inRect.width, 28);

        var buttonsRect = new Rect(0, inRect.yMax - Widgets.BackButtonHeight, inRect.width, Widgets.BackButtonHeight);

        Text.Font = GameFont.Small;
        var totalsHeight = Text.LineHeight * 2f * 1.1f + 8f;
        var totalsRect = new Rect(0, buttonsRect.yMin - totalsHeight - StandardMargin / 2, inRect.width, totalsHeight);

        inRect.yMin = thingRect.yMax + StandardMargin;
        inRect.yMax = totalsRect.yMin - StandardMargin;

        Text.Font = GameFont.Medium;
        GUI.color = Color.cyan;
        Widgets.Label(titleRect, "JAI.Printer.Print.Title".Translate());

        _thing.Draw(thingRect, false, $", HP: {_thing.HitPoints}/{_thing.MaxHitPoints}, Integrity: {_comp.Integrity:0.00}");

        var instructions = _printer.Member.Grid
            .GetComps<Comp_Database>()
            .SelectMany(database => database.Instructions)
            .Where(instruction => instruction.IsThingApplicable(_thing, _comp));
        instructions.Draw(inRect, i => _instruction == i, i => _instruction == i ? [_dequeueButton] : [_enqueueButton]);

        DrawTotals(totalsRect);

        DrawButtons(buttonsRect);

        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
    }

    private void DrawButtons(Rect buttonsRect)
    {
        if (Widgets.ButtonText(buttonsRect with { width = 100 }, "Back".Translate(), overrideTextAnchor: TextAnchor.MiddleCenter)) _selector.ForceClose();

        if (_instruction is null) return;

        var confirmButtonRect = buttonsRect with { xMin = buttonsRect.width - 100 };

        var (damageAmount, breakChance) = _comp.GetApplyDamageFor(_instruction);

        if (breakChance > 0f)
            GUI.color = ArchotechInfusionsMod.ButtonWarningColor;

        var isOutOfArchite = _architeLeft < _architeNeeded;

        if (isOutOfArchite)
            GUI.color = Color.grey;

        if (Widgets.ButtonText(confirmButtonRect, "JAI.Printer.Print.Confirm".Translate(), true, true, !isOutOfArchite, TextAnchor.MiddleCenter))
        {
            var parts = new List<string>
            {
                "JAI.Printer.Print.Desc".Translate(),
                _comp.IsUnbreakable ? "JAI.Printer.Print.Damage.Unbreakable".Translate() : "JAI.Printer.Print.Damage".Translate(damageAmount)
            };

            if (breakChance > 0f)
            {
                var rounding = 10f + Random.value * 20f;
                var estimateChance = Mathf.Round(breakChance * 100f / rounding) * rounding;
                parts.Add("JAI.Printer.Print.BreakChance".Translate(estimateChance));
            }

            Find.WindowStack.Add(
                Dialog_MessageBox.CreateConfirmation(
                    parts.Join(null, "\n\n"),
                    Print,
                    true,
                    "JAI.Printer.Print.Confirm.Title".Translate()
                )
            );
        }

        if (isOutOfArchite) TooltipHandler.TipRegion(confirmButtonRect, "JAI.Printer.Print.NoArchite".Translate(_architeNeeded, _architeLeft));

        GUI.color = Color.white;
    }

    private void Print()
    {
        SoundDefOf.Crunch.PlayOneShotOnCamera();
        _printer.SetInstruction(_instruction, _thing);
        _selector.ForceClose();
    }

    private void DrawTotals(Rect inRect)
    {
        if (_instruction is null) return;

        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        Text.Font = GameFont.Small;
        Widgets.DrawMenuSection(inRect);
        var textRect = inRect.ContractedBy(4);

        var sb = new StringBuilder();
        sb.Append(_instruction.Label).AppendLine(". ");

        sb.Append("JAI.instruction.complexity".Translate()).Append(": ").Append(_instruction.Complexity.ToString("0.00")).Append(". ")
            .Append("JAI.Printer.Print.ArchiteUsed".Translate(_architeNeeded, _architeLeft)).AppendLine(".");

        Widgets.Label(textRect, sb.ToString());
    }
}
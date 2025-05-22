using System.Linq;
using System.Text;
using ArchotechInfusions.comps;
using ArchotechInfusions.injected;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui.component;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArchotechInfusions.ui.print;

public class PrintWindow : Window
{
    private static readonly Color ButtonWarningColor = new(1f, 0.3f, 0.35f);

    private readonly InstructionView.ButtonData _dequeueButton;
    private readonly InstructionView.ButtonData _enqueueButton;

    private readonly Comp_Printer _printer;
    private readonly PrintWindowSelector _selector;
    private readonly Thing _thing;
    private readonly Comp_ArchInfused _comp;
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

        _dequeueButton = new InstructionView.ButtonData { Tooltip = "Do not print this instruction", Texture = TexButton.CloseXSmall, OnClick = OnDequeue };
        _enqueueButton = new InstructionView.ButtonData { Tooltip = "Print this instruction", Texture = TexButton.Play, OnClick = OnEnqueue };
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
        
        _thing.Draw(thingRect, false, $", HP: {_thing.HitPoints}/{_thing.MaxHitPoints}, Integrity: {_comp.Integrity:0.00}"); // todo integrity or not used

        var instructions = _printer.Member.Grid
            .GetComps<Comp_Database>()
            .SelectMany(database => database.Instructions)
            .Where(instruction => instruction.IsThingApplicable(_thing));
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

        if (_comp.Integrity < _instruction.Complexity)
            GUI.color = ButtonWarningColor;

        if (Widgets.ButtonText(confirmButtonRect, "JAI.Printer.Print.Confirm".Translate(), true, true, true, TextAnchor.MiddleCenter))
        {
            if (_comp.Integrity < _instruction.Complexity)
            {
                Find.WindowStack.Add(
                    Dialog_MessageBox.CreateConfirmation(
                        "JAI.Printer.Print.Confirm".Translate(),
                        Print,
                        true,
                        "JAI.Printer.Print.Confirm.Title".Translate()
                    )
                );
            }
            else
                Print();
        }

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
        sb.Append("Complexity: ").Append(_instruction.Complexity.ToString("0.00")).Append(". Archite needed: ").Append(_printer.Props.PrintArchiteCost).AppendLine(".");
        Widgets.Label(textRect, sb.ToString());
    }
}
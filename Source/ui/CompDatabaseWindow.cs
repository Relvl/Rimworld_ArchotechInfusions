using System.Linq;
using ArchotechInfusions.comps;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui.component;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.ui;

public class CompDatabaseWindow : Window
{
    private readonly Comp_Database _comp;
    private readonly InstructionView.ButtonData _deleteButton;

    public override Vector2 InitialSize => new(800, 700);

    public CompDatabaseWindow(Comp_Database comp)
    {
        draggable = true;
        resizeable = true;
        doCloseX = true;

        _comp = comp;
        _deleteButton = new InstructionView.ButtonData { Tooltip = "Delete this instruction", Texture = TexButton.CloseXSmall, OnClick = OnDelete };
    }

    private void OnDelete(AInstruction instruction)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            _comp.RemoveModifier(instruction);
        else
            Find.WindowStack.Add(
                Dialog_MessageBox.CreateConfirmation(
                    "ArchInf.Message.RemoveInstruction".Translate(),
                    () => _comp.RemoveModifier(instruction),
                    true,
                    "ArchInf.Message.RemoveInstruction.Title".Translate()
                )
            );
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        var titleRect = new Rect(0, 0, inRect.width, Text.LineHeight);

        GUI.color = Color.cyan;
        Widgets.Label(titleRect, _comp.parent.LabelCap);

        inRect.yMin = titleRect.yMax + StandardMargin;

        Text.Font = GameFont.Tiny;
        Text.WordWrap = false;
        GUI.color = Color.white;

        var instructions = _comp.Modifiers.ToList();
        instructions.Draw(inRect, _ => false, _ => [_deleteButton]);

        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
    }
}
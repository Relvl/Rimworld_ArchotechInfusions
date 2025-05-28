using System.Linq;
using ArchotechInfusions.building;
using ArchotechInfusions.instructions;
using ArchotechInfusions.ui.component;
using UnityEngine;
using Verse;

namespace ArchotechInfusions.ui;

public class DatabaseWindow : Window
{
    private readonly InstructionView.ButtonData _deleteButton;
    private readonly InstructionDatabase _instructionDatabase;

    public DatabaseWindow(InstructionDatabase instructionDatabase)
    {
        draggable = true;
        resizeable = true;
        doCloseX = true;

        _instructionDatabase = instructionDatabase;
        _deleteButton = new InstructionView.ButtonData { Tooltip = "Delete this instruction", Texture = TexButton.CloseXSmall, OnClick = OnDelete };
    }

    public override Vector2 InitialSize => new(800, 700);

    private void OnDelete(AInstruction instruction)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            _instructionDatabase.RemoveModifier(instruction);
        else
            Find.WindowStack.Add(
                Dialog_MessageBox.CreateConfirmation(
                    "ArchInf.Message.RemoveInstruction".Translate(),
                    () => _instructionDatabase.RemoveModifier(instruction),
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
        Widgets.Label(titleRect, _instructionDatabase.LabelCap);

        inRect.yMin = titleRect.yMax + StandardMargin;

        Text.Font = GameFont.Tiny;
        Text.WordWrap = false;
        GUI.color = Color.white;

        var instructions = _instructionDatabase.Instructions.ToList();
        instructions.Draw(inRect, _ => false, _ => [_deleteButton]);

        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.WordWrap = true;
    }
}
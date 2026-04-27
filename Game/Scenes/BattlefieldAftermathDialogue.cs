using Godot;
using Godot.Collections;

namespace Game.Scenes;

public partial class BattlefieldAftermathDialogue : Node3D
{
    [Export]
    public Resource IntroDialogue { get; set; } = null!;

    [Export]
    public string StartTitle { get; set; } = "battlefield_aftermath";

    /// <summary> Turn off for sandbox maps (e.g. <c>test_scene</c>) so UI does not block clicks. </summary>
    [Export]
    public bool ShowIntroDialogue { get; set; } = true;

    public override void _Ready()
    {
        if (!ShowIntroDialogue)
            return;

        IntroDialogue ??= GD.Load<Resource>("res://Dialogue/Intro.dialogue");

        var dialogueManager = Engine.GetSingleton("DialogueManager");
        if (dialogueManager == null || IntroDialogue == null)
        {
            return;
        }

        dialogueManager.Call("show_dialogue_balloon", IntroDialogue, StartTitle, new Array<Variant>());
    }
}

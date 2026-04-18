using Godot;
using Godot.Collections;

namespace Game.Scenes;

public partial class CouncilRoomIntroTrigger : Node3D
{
    [Export]
    public Resource IntroDialogue { get; set; } = null!;

    [Export]
    public string StartTitle { get; set; } = "start";

    public override void _Ready()
    {
        IntroDialogue ??= GD.Load<Resource>("res://Dialogue/Intro.dialogue");

        var dialogueManager = Engine.GetSingleton("DialogueManager");
        if (dialogueManager == null || IntroDialogue == null)
        {
            return;
        }

        dialogueManager.Call("show_dialogue_balloon", IntroDialogue, StartTitle, new Array<Variant>());
    }
}

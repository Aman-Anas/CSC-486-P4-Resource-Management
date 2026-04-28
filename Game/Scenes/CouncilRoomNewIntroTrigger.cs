using Godot;
using Godot.Collections;

namespace Game.Scenes;

public partial class CouncilRoomNewIntroTrigger : Node3D
{
    [Export] public Resource IntroDialogue { get; set; } = null!;

    [Export] public string StartTitle { get; set; } = "start";

    [Export] private Camera3D WideCamera = null!;
    [Export] private Camera3D CloseCamera = null!;
    [Export] private AnimationPlayer BobAnimPlayer = null!;
    [Export] private StringName BobIdleAnimation = "UAL1_Standard/Idle";

    public override void _Ready()
    {
        AddToGroup("council_room_intro_trigger");
        SwitchToWideCamera();

        BobAnimPlayer.Play(BobIdleAnimation);

        var dialogueManager = Engine.GetSingleton("DialogueManager");
        if (dialogueManager == null || IntroDialogue == null) return;
        dialogueManager.Call("show_dialogue_balloon", IntroDialogue, StartTitle, new Array<Variant>());
    }

    public void SwitchToWideCamera()
    {
        WideCamera?.MakeCurrent();
    }

    public void SwitchToCloseCamera()
    {
        CloseCamera?.MakeCurrent();
    }
}

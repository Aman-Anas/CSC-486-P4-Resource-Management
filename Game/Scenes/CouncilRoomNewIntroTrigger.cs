using Godot;
using Godot.Collections;

namespace Game.Scenes;

public partial class CouncilRoomNewIntroTrigger : Node3D
{
    [Export] public Resource IntroDialogue { get; set; } = null!;

    [Export] public string StartTitle { get; set; } = "start";
    
    [Export] private Camera3D WideCamera;
    [Export] private Camera3D CloseCamera;

    public override void _Ready()
    {
        WideCamera.MakeCurrent();
        
        var dialogueManager = Engine.GetSingleton("DialogueManager");
        if (dialogueManager == null || IntroDialogue == null) return;
        dialogueManager.Call("show_dialogue_balloon", IntroDialogue, StartTitle, new Array<Variant>());
    }
}

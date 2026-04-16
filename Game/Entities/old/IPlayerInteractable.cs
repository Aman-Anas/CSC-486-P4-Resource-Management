using Godot;

namespace Game.Entities;

/// <summary>
/// Interface for objects the player can interact with using the existing [e] action.
/// </summary>
public interface IPlayerInteractable
{
    string GetInteractionText(Farmer farmer);

    Color GetInteractionColor();

    void Interact(Farmer farmer);
}

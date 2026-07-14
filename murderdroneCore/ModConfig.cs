using System.Runtime.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace MURDERDRONE;

/// <summary>The player-configurable mod settings.</summary>
public sealed class ModConfig
{
    /// <summary>Whether the drone starts enabled for a player who has no saved preference.</summary>
    public bool Active { get; set; } = true;

    /// <summary>The keyboard or controller bindings which toggle the current player's drone.</summary>
    public KeybindList ToggleKey { get; set; } = CreateDefaultToggleKey();

    public int RotationSpeed { get; set; } = 10;

    public float DroneRadius { get; set; } = 120f;

    public int Damage { get; set; } = -1;

    public int ProjectileVelocity { get; set; } = 16;

    /// <summary>Restore every option to its default value.</summary>
    internal void Reset()
    {
        this.Active = true;
        this.ToggleKey = CreateDefaultToggleKey();
        this.RotationSpeed = 10;
        this.DroneRadius = 120f;
        this.Damage = -1;
        this.ProjectileVelocity = 16;
    }

    /// <summary>Normalize values which JSON may explicitly set to null.</summary>
    [OnDeserialized]
    private void OnDeserialized(StreamingContext _)
    {
        this.ToggleKey ??= new KeybindList();
    }

    internal static KeybindList CreateDefaultToggleKey()
    {
        return KeybindList.Parse($"{SButton.F7}, {SButton.LeftStick}");
    }
}

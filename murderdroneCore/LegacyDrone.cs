using Microsoft.Xna.Framework;
using StardewValley;

namespace MURDERDRONE;

/// <summary>The pre-1.4 NPC type, retained only so old saves can load and remove it safely.</summary>
[Obsolete("Legacy save compatibility only. Use CombatDroneCompanion.")]
public sealed class Drone : NPC
{
    /// <summary>Construct an empty legacy drone for save deserialization.</summary>
    public Drone()
        : base(new AnimatedSprite("Sidekick/Drone", 1, 12, 12), Vector2.Zero, 1, "Drone")
    {
        this.hideShadow.Value = true;
    }

    /// <inheritdoc />
    public override void update(GameTime time, GameLocation location)
    {
    }
}

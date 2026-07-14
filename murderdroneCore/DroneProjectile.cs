using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace MURDERDRONE;

/// <summary>A drone projectile which lets the normal game damage pipeline handle every hit once.</summary>
public sealed class DroneProjectile : BasicProjectile
{
    private IReflectionHelper? reflection;

    /// <summary>Construct an empty instance for the game's network serializer.</summary>
    public DroneProjectile()
    {
    }

    internal DroneProjectile(
        int damage,
        Vector2 velocity,
        Vector2 startingPosition,
        GameLocation location,
        Farmer owner,
        IReflectionHelper? reflection
    )
        : base(
            damageToFarmer: damage,
            spriteIndex: Projectile.shadowBall,
            bouncesTillDestruct: 0,
            tailLength: 0,
            rotationVelocity: 0f,
            xVelocity: velocity.X,
            yVelocity: velocity.Y,
            startingPosition: startingPosition,
            collisionSound: "hitEnemy",
            firingSound: "daggerswipe",
            explode: false,
            damagesMonsters: true,
            location: location,
            firer: owner
        )
    {
        this.reflection = reflection;
    }

    /// <inheritdoc />
    public override void behaviorOnCollisionWithMonster(NPC target, GameLocation location)
    {
        if (target is Bug bug && bug.isArmoredBug.Value)
        {
            bool wasArmored = bug.isArmoredBug.Value;
            bug.isArmoredBug.Value = false;
            try
            {
                base.behaviorOnCollisionWithMonster(target, location);
            }
            finally
            {
                bug.isArmoredBug.Value = wasArmored;
            }

            return;
        }

        if (target is RockCrab rockCrab && this.reflection is not null)
        {
            NetBool shellGone = this.reflection.GetField<NetBool>(rockCrab, "shellGone").GetValue();
            if (!shellGone.Value)
            {
                shellGone.Value = true;
                this.reflection.GetField<NetInt>(rockCrab, "shellHealth").GetValue().Value = 0;
            }
        }

        base.behaviorOnCollisionWithMonster(target, location);
    }
}

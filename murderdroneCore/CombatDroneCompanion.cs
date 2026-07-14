using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace MURDERDRONE;

/// <summary>A combat drone owned and synchronized by one farmer.</summary>
public sealed class CombatDroneCompanion : Companion
{
    private const string TextureAssetName = "Sidekick/Drone";
    private const int AttackRangeInTiles = 9;
    private const int MaxConcurrentProjectiles = 10;
    private const int MaxProjectileTravelDistance = 900;

    private static readonly Rectangle SourceRectangle = new(12, 0, 12, 12);
    private static readonly Vector2 SpriteOrigin = new(6f, 6f);
    private static readonly Vector2 ProjectilePositionOffset = new(32f, 32f);

    private readonly List<TrackedProjectile> projectiles = new();
    private readonly List<TargetCandidate> targetCandidates = new();

    private IReflectionHelper? reflection;
    private float orbitProgress;
    private int rotationSpeed = 10;
    private float droneRadius = 120f;
    private int damage = -1;
    private float projectileVelocity = 16f;

    /// <summary>Construct an empty instance for the game's network serializer.</summary>
    public CombatDroneCompanion()
    {
    }

    /// <summary>Construct a drone using the current mod settings.</summary>
    internal CombatDroneCompanion(ModConfig config, IReflectionHelper reflection)
    {
        this.ApplySettings(config, reflection);
    }

    /// <summary>Apply settings which are only needed by the owning local player.</summary>
    internal void ApplySettings(ModConfig config, IReflectionHelper reflection)
    {
        this.rotationSpeed = Math.Max(1, config.RotationSpeed);
        this.droneRadius = config.DroneRadius;
        this.damage = config.Damage;
        this.projectileVelocity = config.ProjectileVelocity;
        this.reflection = reflection;
    }

    /// <inheritdoc />
    public override void Update(GameTime time, GameLocation location)
    {
        Farmer? owner = this.Owner;
        if (owner is null || !this.IsLocal || owner.currentLocation != location)
            return;

        this.RemoveFinishedProjectiles(location);

        if (!IsAllowed(location))
            return;

        this.UpdatePosition(time);
        this.FindTargets(location);

        foreach (TargetCandidate candidate in this.targetCandidates)
        {
            if (this.projectiles.Count >= MaxConcurrentProjectiles)
                break;

            this.Shoot(location, candidate.Target);
        }
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        Farmer? owner = this.Owner;
        if (owner?.currentLocation is null || !IsAllowed(owner.currentLocation))
            return;

        Texture2D texture = Game1.content.Load<Texture2D>(TextureAssetName);
        Vector2 drawPosition = Game1.GlobalToLocal(this.Position + owner.drawOffset);
        float layerDepth = Math.Clamp(this.Position.Y / 10000f, 0f, 1f);

        spriteBatch.Draw(
            texture,
            drawPosition,
            SourceRectangle,
            Color.White,
            0f,
            SpriteOrigin,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth
        );
    }

    /// <inheritdoc />
    public override void OnOwnerWarp()
    {
        this.DestroyTrackedProjectiles();
        base.OnOwnerWarp();
    }

    /// <inheritdoc />
    public override void CleanupCompanion()
    {
        this.DestroyTrackedProjectiles();
        base.CleanupCompanion();
    }

    private static bool IsAllowed(GameLocation location)
    {
        return location is not DecoratableLocation;
    }

    private void UpdatePosition(GameTime time)
    {
        float angle = this.orbitProgress * MathF.Tau;
        this.Position = this.OwnerPosition + new Vector2(
            5f + this.droneRadius * MathF.Cos(angle),
            -20f + this.droneRadius * MathF.Sin(angle)
        );

        float elapsedMilliseconds = (float)time.ElapsedGameTime.TotalMilliseconds;
        this.orbitProgress = (this.orbitProgress + elapsedMilliseconds / (100f * this.rotationSpeed)) % 1f;
    }

    private void FindTargets(GameLocation location)
    {
        float maxDistance = AttackRangeInTiles * Game1.tileSize;
        float maxDistanceSquared = maxDistance * maxDistance;
        this.targetCandidates.Clear();

        foreach (Character character in location.characters)
        {
            if (character is not Monster monster || monster.Health <= 0 || monster.IsInvisible)
                continue;

            float distanceSquared = Vector2.DistanceSquared(monster.Position, this.Owner.Position);
            if (distanceSquared <= maxDistanceSquared && !this.HasProjectileFor(monster))
                this.targetCandidates.Add(new TargetCandidate(monster, distanceSquared));
        }

        this.targetCandidates.Sort(static (left, right) =>
            left.DistanceSquared.CompareTo(right.DistanceSquared)
        );
    }

    private bool HasProjectileFor(Monster target)
    {
        return this.projectiles.Any(projectile => ReferenceEquals(projectile.Target, target));
    }

    private void RemoveFinishedProjectiles(GameLocation location)
    {
        for (int index = this.projectiles.Count - 1; index >= 0; index--)
        {
            TrackedProjectile tracked = this.projectiles[index];
            DroneProjectile projectile = tracked.Projectile;
            bool targetIsGone = tracked.Target.Health <= 0 || tracked.Target.currentLocation != location;
            bool projectileIsDone = projectile.destroyMe
                                    || projectile.travelDistance >= projectile.maxTravelDistance.Value
                                    || !location.projectiles.Contains(projectile);

            if (!targetIsGone && !projectileIsDone)
                continue;

            if (targetIsGone && !projectileIsDone)
                location.projectiles.Remove(projectile);

            this.projectiles.RemoveAt(index);
        }
    }

    private void Shoot(GameLocation location, Monster monster)
    {
        int shotDamage = this.damage == -1
            ? Math.Max(1, (int)Math.Ceiling(monster.Health * 1.2d))
            : this.damage;

        Vector2 targetPosition = Utility.PointToVector2(monster.GetBoundingBox().Center);
        Vector2 velocity = Utility.getVelocityTowardPoint(
            this.Position,
            targetPosition,
            this.projectileVelocity
        );

        DroneProjectile projectile = new(
            shotDamage,
            velocity,
            this.Position - ProjectilePositionOffset,
            location,
            this.Owner,
            this.reflection
        )
        {
            IgnoreLocationCollision = location.currentEvent is not null
        };
        projectile.maxTravelDistance.Value = MaxProjectileTravelDistance;

        location.projectiles.Add(projectile);
        this.projectiles.Add(new TrackedProjectile(monster, projectile, location));
    }

    private void DestroyTrackedProjectiles()
    {
        foreach (TrackedProjectile tracked in this.projectiles)
            tracked.Location.projectiles.Remove(tracked.Projectile);

        this.projectiles.Clear();
    }

    private readonly record struct TargetCandidate(Monster Target, float DistanceSquared);

    private sealed record TrackedProjectile(
        Monster Target,
        DroneProjectile Projectile,
        GameLocation Location
    );
}

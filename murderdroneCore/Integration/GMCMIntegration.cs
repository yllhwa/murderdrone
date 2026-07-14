using GenericModConfigMenu;
using MURDERDRONE;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MURDERDRONE.Integration;

/// <summary>Registers this mod's settings with Generic Mod Config Menu when available.</summary>
internal sealed class GMCMIntegration
{
    private readonly IModHelper helper;
    private readonly IManifest manifest;
    private readonly ModConfig config;
    private readonly Action onSaved;

    public GMCMIntegration(
        IModHelper helper,
        IManifest manifest,
        ModConfig config,
        Action onSaved
    )
    {
        this.helper = helper;
        this.manifest = manifest;
        this.config = config;
        this.onSaved = onSaved;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IGenericModConfigMenuApi? menu = this.helper.ModRegistry
            .GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (menu is null)
            return;

        menu.Register(
            mod: this.manifest,
            reset: this.config.Reset,
            save: () =>
            {
                this.helper.WriteConfig(this.config);
                this.onSaved();
            }
        );

        menu.AddSectionTitle(
            mod: this.manifest,
            text: () => this.helper.Translation.Get("config.section")
        );
        menu.AddKeybindList(
            mod: this.manifest,
            name: () => this.helper.Translation.Get("config.toggle-key.name"),
            tooltip: () => this.helper.Translation.Get("config.toggle-key.tooltip"),
            getValue: () => this.config.ToggleKey,
            setValue: value => this.config.ToggleKey = value
        );
        menu.AddBoolOption(
            mod: this.manifest,
            name: () => this.helper.Translation.Get("config.active.name"),
            tooltip: () => this.helper.Translation.Get("config.active.tooltip"),
            getValue: () => this.config.Active,
            setValue: value => this.config.Active = value
        );
        menu.AddNumberOption(
            mod: this.manifest,
            name: () => this.helper.Translation.Get("config.rotation-speed.name"),
            tooltip: () => this.helper.Translation.Get("config.rotation-speed.tooltip"),
            getValue: () => this.config.RotationSpeed,
            setValue: value => this.config.RotationSpeed = Math.Max(1, value),
            min: 1
        );
        menu.AddNumberOption(
            mod: this.manifest,
            name: () => this.helper.Translation.Get("config.radius.name"),
            tooltip: () => this.helper.Translation.Get("config.radius.tooltip"),
            getValue: () => this.config.DroneRadius,
            setValue: value => this.config.DroneRadius = value
        );
        menu.AddNumberOption(
            mod: this.manifest,
            name: () => this.helper.Translation.Get("config.damage.name"),
            tooltip: () => this.helper.Translation.Get("config.damage.tooltip"),
            getValue: () => this.config.Damage,
            setValue: value => this.config.Damage = value
        );
        menu.AddNumberOption(
            mod: this.manifest,
            name: () => this.helper.Translation.Get("config.projectile-velocity.name"),
            tooltip: () => this.helper.Translation.Get("config.projectile-velocity.tooltip"),
            getValue: () => this.config.ProjectileVelocity,
            setValue: value => this.config.ProjectileVelocity = value
        );
    }
}

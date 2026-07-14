using Microsoft.Xna.Framework.Graphics;
using MURDERDRONE.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MURDERDRONE;

/// <summary>The mod entry point.</summary>
public sealed class ModEntry : Mod
{
    private const string EnabledModDataKey = "prism99.cambam.MURDERDRONE.redux/Enabled";
    private const string LegacyDroneNamePrefix = "Drone_";

    private ModConfig config = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.config = this.ReadConfig(helper);
        _ = new GMCMIntegration(helper, this.ModManifest, this.config, this.ApplyConfigToDrones);

        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    private ModConfig ReadConfig(IModHelper helper)
    {
        LegacyConfig? legacy = helper.Data.ReadJsonFile<LegacyConfig>("config.json");
        ModConfig result = helper.ReadConfig<ModConfig>();

        if (legacy is not null && string.IsNullOrWhiteSpace(legacy.ToggleKey))
        {
            string previousBinding = legacy.Keybind != SButton.None
                ? legacy.Keybind.ToString()
                : !string.IsNullOrWhiteSpace(legacy.KeyboardShortcut)
                    ? legacy.KeyboardShortcut
                    : SButton.F7.ToString();

            result.ToggleKey = StardewModdingAPI.Utilities.KeybindList.Parse(
                $"{previousBinding}, {SButton.LeftStick}"
            );
            helper.WriteConfig(result);
        }

        return result;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("Sidekick/Drone"))
        {
            e.LoadFromModFile<Texture2D>(
                "Assets/drone_sprite_robot.png",
                AssetLoadPriority.Medium
            );
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (Context.IsMainPlayer)
            this.RemoveLegacyDrones();

        this.ReconcileDrone(Game1.player, this.GetEnabled(Game1.player));
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        this.ReconcileDrone(Game1.player, this.GetEnabled(Game1.player));
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsWorldReady
            || !Context.IsPlayerFree
            || Game1.currentMinigame is not null
            || !this.config.ToggleKey.JustPressed())
        {
            return;
        }

        Farmer player = Game1.player;
        bool enabled = !this.GetEnabled(player);
        this.SetEnabled(player, enabled);

        if (enabled)
            Game1.addHUDMessage(new HUDMessage(this.Helper.Translation.Get("message.activated"), 4));
        else
            Game1.showRedMessage(this.Helper.Translation.Get("message.deactivated"));
    }

    private bool GetEnabled(Farmer player)
    {
        if (player.modData.TryGetValue(EnabledModDataKey, out string? value)
            && bool.TryParse(value, out bool enabled))
        {
            return enabled;
        }

        this.WriteEnabled(player, this.config.Active);
        return this.config.Active;
    }

    private void SetEnabled(Farmer player, bool enabled)
    {
        this.WriteEnabled(player, enabled);
        this.ReconcileDrone(player, enabled);
    }

    private void WriteEnabled(Farmer player, bool enabled)
    {
        player.modData[EnabledModDataKey] = enabled.ToString();
    }

    private void ReconcileDrone(Farmer player, bool enabled)
    {
        List<CombatDroneCompanion> drones = player.companions
            .OfType<CombatDroneCompanion>()
            .ToList();

        if (!enabled)
        {
            foreach (CombatDroneCompanion drone in drones)
                player.RemoveCompanion(drone);

            return;
        }

        CombatDroneCompanion activeDrone;
        if (drones.Count == 0)
        {
            activeDrone = new CombatDroneCompanion(this.config, this.Helper.Reflection);
            player.AddCompanion(activeDrone);
        }
        else
        {
            activeDrone = drones[0];
            activeDrone.ApplySettings(this.config, this.Helper.Reflection);
        }

        foreach (CombatDroneCompanion duplicate in drones.Skip(1))
            player.RemoveCompanion(duplicate);
    }

    private void ApplyConfigToDrones()
    {
        if (!Context.IsWorldReady)
            return;

        foreach (Farmer farmer in Game1.getAllFarmers())
        {
            foreach (CombatDroneCompanion drone in farmer.companions.OfType<CombatDroneCompanion>())
                drone.ApplySettings(this.config, this.Helper.Reflection);
        }
    }

    private void RemoveLegacyDrones()
    {
        int removed = 0;

        Utility.ForEachLocation(
            location =>
            {
#pragma warning disable CS0618
                foreach (NPC character in location.characters.ToList())
                {
                    if (character is Drone
                        || (character.Name.StartsWith(LegacyDroneNamePrefix, StringComparison.Ordinal)
                            && character.modData.ContainsKey("mdrone.playerId")))
                    {
                        location.characters.Remove(character);
                        removed++;
                    }
                }
#pragma warning restore CS0618
                return true;
            },
            includeGenerated: true
        );

        if (removed > 0)
            this.Monitor.Log($"Removed {removed} legacy NPC drone(s).", LogLevel.Info);
    }

    private sealed class LegacyConfig
    {
        public string? ToggleKey { get; set; }

        public string? KeyboardShortcut { get; set; }

        public SButton Keybind { get; set; }
    }
}

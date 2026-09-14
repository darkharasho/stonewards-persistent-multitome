using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PersistentMultiTome
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // Both mods sharing the same pickup would give every other player the upgrade twice.
    [BepInIncompatibility(MultiTomeGuid)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.darkharasho.stonewards.persistentmultitome";
        public const string PluginName = "PersistentMultiTome";
        public const string PluginVersion = "0.1.1";
        public const string MultiTomeGuid = "rei.stonewards.multitome";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> SharePickupUpgrades;
        internal static ConfigEntry<bool> ShareRingUpgrades;
        internal static ConfigEntry<bool> ShareServerWideUpgrades;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            // Display names and order are for the in-game ModSettings menu; they don't change the .cfg.
            SharePickupUpgrades = Config.Bind("Sharing", "SharePickupUpgrades", true, new ConfigDescription(
                "Share tomes picked up in the level with every player.", null,
                new ConfigurationManagerAttributes { DispName = "Share tomes", Order = 30 }));
            ShareRingUpgrades = Config.Bind("Sharing", "ShareRingUpgrades", false, new ConfigDescription(
                "Share rings chosen on level-up with every player.", null,
                new ConfigurationManagerAttributes { DispName = "Share rings", Order = 20 }));
            ShareServerWideUpgrades = Config.Bind("Sharing", "ShareServerWideUpgrades", false, new ConfigDescription(
                "Also share upgrades the base game already applies to the whole team. Their effect then stacks once per player.", null,
                new ConfigurationManagerAttributes { DispName = "Share upgrades the game already shares", Order = 10 }));

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            Log.LogInfo($"{PluginName} {PluginVersion} loaded. Only the host needs it.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}

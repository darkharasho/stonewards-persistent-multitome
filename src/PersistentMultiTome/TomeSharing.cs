using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Mirror;
using MyBox;

namespace PersistentMultiTome
{
    /// <summary>
    /// Host-side sharing. Each shared upgrade is applied and saved to the receiver's run record exactly as the game
    /// does for their own pick, so the game's reconnect restore gives it back without any help.
    /// </summary>
    internal static class TomeSharing
    {
        private static readonly ShareLedger ledger = new ShareLedger();

        // Players whose run record didn't exist until they spawned: first time in this run.
        private static readonly HashSet<ulong> firstJoins = new HashSet<ulong>();

        private static string LedgerPath => Path.Combine(Paths.ConfigPath, "PersistentMultiTome.shared.txt");

        private static Dictionary<ulong, RunPlayerData> RunPlayers => Singleton<SaveManager>.Instance.saveContainer.runPlayersData;

        /// <summary>After the game applied and saved <paramref name="picker"/>'s own pick.</summary>
        public static void OnUpgradePicked(FirstPersonController picker, string upgradeID, Rarity rarity)
        {
            if (!NetworkServer.active || picker == null || RogueLikeUpgradeManager.Instance == null) return;
            var upgrade = RogueLikeUpgradeManager.Instance.GetUpgradeSOByID(upgradeID);
            if (upgrade == null || !ShouldShare(upgrade)) return;

            ledger.Add(new ShareLedger.Entry(picker.playerID, upgradeID, (int)rarity));

            var online = new HashSet<ulong> { picker.playerID };
            foreach (var player in OnlinePlayers())
            {
                if (!online.Add(player.playerID)) continue;
                Grant(player, upgrade, rarity);
            }

            // Players who left mid-run get it from the game's restore when they reconnect.
            var offline = new List<RunPlayerData>();
            foreach (var pair in RunPlayers)
            {
                if (!online.Contains(pair.Key)) offline.Add(pair.Value);
            }
            foreach (var record in offline) Record(record, upgradeID, rarity);

            Plugin.Log.LogInfo($"Shared {upgrade.UpgradeCategory} {upgradeID} ({rarity}) with {online.Count - 1} online and {offline.Count} offline players.");
        }

        /// <summary>Before the game creates <paramref name="player"/>'s run record on spawn.</summary>
        public static void BeforeServerInit(FirstPersonController player)
        {
            if (!NetworkServer.active) return;
            if (RunPlayers.ContainsKey(player.playerID)) firstJoins.Remove(player.playerID);
            else firstJoins.Add(player.playerID);
        }

        /// <summary>After the game spawned a player and restored their saved upgrades.</summary>
        public static void AfterPlayerAdded(NetworkConnectionToClient conn)
        {
            if (!NetworkServer.active || conn?.identity == null) return;
            var player = conn.identity.GetComponent<FirstPersonController>();
            if (player == null || !firstJoins.Remove(player.playerID)) return;
            if (ledger.Entries.Count == 0 || RogueLikeUpgradeManager.Instance == null) return;

            var given = 0;
            foreach (var entry in ledger.BacklogFor(player.playerID))
            {
                var upgrade = RogueLikeUpgradeManager.Instance.GetUpgradeSOByID(entry.UpgradeID);
                if (upgrade == null) continue;
                Grant(player, upgrade, (Rarity)entry.Rarity);
                given++;
            }
            Plugin.Log.LogInfo($"Gave {given} previously shared upgrades to {player.playerID}, who joined mid-run.");
        }

        public static void OnRunDataCleared()
        {
            ledger.Clear();
            firstJoins.Clear();
        }

        public static void OnSaved(SaveManager.SaveContainer container)
        {
            try
            {
                File.WriteAllText(LedgerPath, ledger.Serialize(container.lastSaveTimeUtc.Ticks));
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Could not save shared upgrades to {LedgerPath}: {e.Message}");
            }
        }

        public static void OnLoaded(SaveManager.SaveContainer container)
        {
            if (container == null || !File.Exists(LedgerPath)) return;
            try
            {
                if (!ledger.TryLoad(File.ReadAllText(LedgerPath), container.lastSaveTimeUtc.Ticks))
                {
                    Plugin.Log.LogWarning("Saved shared upgrades don't match this save; ignoring them.");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Could not load shared upgrades from {LedgerPath}: {e.Message}");
            }
        }

        private static bool ShouldShare(RogueLikeUpgradeSO upgrade) => ShareRules.ShouldShare(
            upgrade.UpgradeCategory == UpgradeCategory.PICKUP, upgrade.IsServer,
            Plugin.SharePickupUpgrades.Value, Plugin.ShareRingUpgrades.Value, Plugin.ShareServerWideUpgrades.Value);

        /// <summary>Same three steps as a real pick: apply the effect, save it, add it to the player's upgrade list.</summary>
        private static void Grant(FirstPersonController player, RogueLikeUpgradeSO upgrade, Rarity rarity)
        {
            RogueLikeUpgradeManager.Instance.ServerUpgradeAcquired(player, upgrade, rarity);
            if (!RunPlayers.TryGetValue(player.playerID, out var record))
            {
                record = new RunPlayerData();
                RunPlayers[player.playerID] = record;
            }
            Record(record, upgrade.ID, rarity);
            if (player.connectionToClient != null)
            {
                NetworkHelper.Instance.TargetRestoreRogueUpgrade(player.connectionToClient, upgrade.ID);
            }
        }

        private static void Record(RunPlayerData record, string upgradeID, Rarity rarity)
        {
            record.rogueUpgrades.Add(new RogueUpgradeData { upgradeID = upgradeID, rarity = rarity });
        }

        private static IEnumerable<FirstPersonController> OnlinePlayers()
        {
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn?.identity == null) continue;
                var player = conn.identity.GetComponent<FirstPersonController>();
                if (player != null) yield return player;
            }
        }
    }
}

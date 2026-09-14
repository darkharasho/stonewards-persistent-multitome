using HarmonyLib;
using Mirror;

namespace PersistentMultiTome.Patches
{
    /// <summary>
    /// The host's handler for a player's upgrade pick. Hooked here rather than in ServerUpgradeAcquired, which the
    /// game also calls when restoring a reconnecting player's upgrades and would share them all again.
    /// </summary>
    [HarmonyPatch(typeof(NetworkHelper), "UserCode_CmdApplyUpgrade__FirstPersonController__String__Rarity")]
    internal static class SharePickedUpgradePatch
    {
        private static void Postfix(FirstPersonController _Player, string _UpgradeID, Rarity _UpgradeRarity)
        {
            TomeSharing.OnUpgradePicked(_Player, _UpgradeID, _UpgradeRarity);
        }
    }

    [HarmonyPatch(typeof(FirstPersonController), nameof(FirstPersonController.ServerInit))]
    internal static class DetectFirstJoinPatch
    {
        private static void Prefix(FirstPersonController __instance)
        {
            TomeSharing.BeforeServerInit(__instance);
        }
    }

    [HarmonyPatch(typeof(StoneWardsNetworkManager), nameof(StoneWardsNetworkManager.OnServerAddPlayer))]
    internal static class GiveBacklogOnJoinPatch
    {
        private static void Postfix(NetworkConnectionToClient conn)
        {
            TomeSharing.AfterPlayerAdded(conn);
        }
    }

    [HarmonyPatch(typeof(StoneWardsNetworkManager), "ClearAllRunData")]
    internal static class ClearOnRunEndPatch
    {
        private static void Postfix()
        {
            TomeSharing.OnRunDataCleared();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "SaveProgression")]
    internal static class SaveLedgerPatch
    {
        private static void Postfix(SaveManager.SaveContainer _saveContainer, bool __result)
        {
            if (__result) TomeSharing.OnSaved(_saveContainer);
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadProgressionSave))]
    internal static class LoadLedgerPatch
    {
        private static void Postfix(SaveManager __instance)
        {
            TomeSharing.OnLoaded(__instance.saveContainer);
        }
    }
}

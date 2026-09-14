namespace PersistentMultiTome
{
    /// <summary>Which picked upgrades get shared. No game types, so it can be unit tested.</summary>
    internal static class ShareRules
    {
        /// <param name="isPickup">Tome picked up in the level; otherwise a ring from a level-up.</param>
        /// <param name="isServerWide">
        /// The game applies it on the host for the whole team, so sharing it stacks the effect once per player.
        /// </param>
        public static bool ShouldShare(bool isPickup, bool isServerWide, bool sharePickups, bool shareRings, bool shareServerWide)
        {
            if (isServerWide && !shareServerWide) return false;
            return isPickup ? sharePickups : shareRings;
        }
    }
}

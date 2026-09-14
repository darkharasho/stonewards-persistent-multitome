# stonewards-persistent-multitome

A Stonewards mod, inspired by TheChiizu's MultiTome, where tomes picked up by any player are shared with everyone in the lobby. It keeps track of every shared tome for each player, so someone who disconnects and reconnects gets all of them back. Each shared tome is granted and stored as if that player had picked it up themselves.

Player-facing docs are in [thunderstore/README.md](thunderstore/README.md).

## How it works

Host only, BepInEx 5 + Harmony. See `src/PersistentMultiTome/TomeSharing.cs`.

- **Share:** postfix on `NetworkHelper.UserCode_CmdApplyUpgrade`, the host's handler for a player's pick. For every other online player it does what the game does for their own pick: `ServerUpgradeAcquired`, append to `saveContainer.runPlayersData[steamID].rogueUpgrades`, and `TargetRestoreRogueUpgrade` so their upgrade list counts it. It doesn't hook `ServerUpgradeAcquired` because the game also calls that when restoring a reconnecting player, and this mod replays shared tomes through the run record instead.
- **Rejoin:** nothing extra. The game's `ServerRestoreRogueUpgrades` replays the run record on spawn. Players in the record but offline at share time get the entry appended directly.
- **First join mid-run:** a prefix on `FirstPersonController.ServerInit` notes players with no run record yet; after `OnServerAddPlayer` they get every entry in the shared ledger.
- **Ledger:** cleared with `ClearAllRunData`, written to `BepInEx/config/PersistentMultiTome.shared.txt` whenever the game saves, and loaded only if it matches the save's `lastSaveTimeUtc`.
- `ShareServerWideUpgrades` gates upgrades with `BaseUpgradeSO.IsServer`, which the game applies to the whole team.

## Building

```sh
dotnet build src/PersistentMultiTome/PersistentMultiTome.csproj -c Release   # also copies the DLL into the r2modman Default profile
dotnet test tests/PersistentMultiTome.Tests
scripts/package.sh                                                          # dist/darkharasho-PersistentMultiTome-<version>.zip
```

Without the game installed the build uses `lib/refs`. After a game update, run `scripts/update-refs.sh`.

## Releasing

1. Bump the version in `thunderstore/manifest.json`, `src/PersistentMultiTome/Plugin.cs` and the `.csproj`, and add a `CHANGELOG.md` entry. `package.sh` checks all four match.
2. Push a `v<version>` tag. `.github/workflows/release.yml` tests, packages, creates the GitHub release and publishes to Thunderstore with the `THUNDERSTORE_TOKEN` secret.

Thunderstore never allows replacing a published version, so a pushed tag goes live. Bump the version for any fix.

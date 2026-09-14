# PersistentMultiTome

When any dwarf picks up a tome, the whole team gets it. Shared tomes are saved to each player's run, so if you disconnect and rejoin you get every one of them back.

## How it works

- Each shared tome is given to every other player exactly as if they had picked it up themselves. It's applied, added to their upgrade list and saved to their record for the run.
- **Rejoining:** the game already gives your own tomes back when you reconnect mid-run. Shared tomes are saved the same way, so they come back too. Tomes shared while you were gone are waiting for you.
- **Joining mid-run:** a player who joins the run for the first time gets every tome shared so far.
- Tome counts in the upgrade list stay correct for everyone, including tomes someone else picked up.
- Shared tomes last for the current run. They are cleared when the run ends, the same as your own.

## Multiplayer

Only the host needs the mod. Other players can have it installed or not; it does nothing unless you are hosting.

Use either this mod or TheChiizu's MultiTome, not both. They do the same job, so together everyone would get each tome twice. If MultiTome is installed, BepInEx won't load this mod.

## Configuration

Edit in r2modman under **Config editor** → `com.darkharasho.stonewards.persistentmultitome.cfg`, in `BepInEx/config`, or in game with ModSettings. Settings are read on the host.

| Setting | Default | Description |
| --- | --- | --- |
| Sharing → SharePickupUpgrades | `true` | Share tomes picked up in the level. |
| Sharing → ShareRingUpgrades | `false` | Share rings chosen on level-up. |
| Sharing → ShareServerWideUpgrades | `false` | Some upgrades already apply to the whole team in the base game. They aren't shared by default, so their effect doesn't stack. Turn on to share them anyway, stacking once per player. |

## Credits

Inspired by and thanks to [MultiTome](https://thunderstore.io/c/stonewards/p/TheChiizu/MultiTome/) by TheChiizu, which came up with the idea. This mod is written separately and shares no code.

## Issues

Report bugs at https://github.com/darkharasho/stonewards-persistent-multitome/issues. Please attach the host's `BepInEx/LogOutput.log`.

## Project Context

A Stonewards mod that recreates TheChiizu's MultiTome, where tomes picked up by any player are shared with everyone in the lobby. Unlike the original, it keeps track of every shared tome for each player, so someone who disconnects and reconnects gets all of them back. Each shared tome is granted and stored as if that player had picked it up themselves.

## Goals

- Share every tome picked up by any player with all other players in the session
- Grant shared tomes as if the receiving player had picked them up themselves
- Keep a record of all tomes each player has received during the run, including shared ones
- Give the full tome set back to a player when they reconnect after disconnecting
- Stay compatible with the usual Thunderstore/r2modman install process

## Out of scope

- Reusing MultiTome's code without checking its license first
- Keeping tomes between separate runs or save files
- Changing how tomes work or how strong they are

## Suggested stack

- **C# / .NET (Unity mod)** — Stonewards is assumed to be a Unity game, and C# is the standard language for its mods
- **BepInEx** — The usual mod loader for Thunderstore-hosted Unity mods (needs confirming for Stonewards)
- **HarmonyX** — Lets the mod hook tome pickup, player join, and reconnect logic without editing game files

# Quickstarts

Skip the menus. Boot straight into a colony that is already set up the way you need it.

Define a scenario in C#, then launch it from the dev quicktest menu or with a command line flag. No scenario picker, no planet, no pawn shuffling, no landing site.

```csharp
public class TinyColony : AbstractQuickstart {
  public override TaggedString description => "One small map, three colonists, paused.";

  public override int mapSize => 50;

  public override void PrepareColonists(List<Pawn> pawns) {
    foreach (Pawn pawn in pawns) {
      pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
    }
  }
}
```

## Who this is for

Mod authors. Quickstarts adds nothing to a normal game, so there is no reason to subscribe unless you are building or testing a mod.

## What you get

A picker on the dev quicktest button, listing every quickstart the loaded mods define. Pick one and you are on the map, paused, with the colony already configured.

Every non-abstract subclass is found automatically. Adding a quickstart means writing a class and nothing else.

Hooks run at the points that matter: before generation, after the world exists, after the scenario finishes its own setup, and once the colonists are spawned. So you can set pawn counts, finish research, hand out gear, or open the panel you are working on.

The same scenario doubles as a smoke test. Run it with a verify flag and the game asserts against the live simulation, writes a JSON report, and exits with a pass or fail code. That works in CI with no display attached.

## Requirements

RimWorld 1.6, dev mode on, and one patching library. Harmony or Concord both work. Quickstarts prefers Concord when you have both.

## Getting started

Reference the RimWorks.Quickstarts.Ref package, subclass AbstractQuickstart, and your quickstart shows up in the picker. To ship one from a mod without forcing this dependency on your players, put it in a folder gated behind loadFolders.xml.

Source and documentation: https://github.com/RimWorks/Rimworld-Quickstarts

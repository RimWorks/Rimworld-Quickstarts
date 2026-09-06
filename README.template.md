# Quickstarts: RimWorld dev quicktest scenarios

Skip the menus and boot straight into a colony that is already set up the way you need it. Quickstarts replaces RimWorld's dev quicktest button with your own scenarios, written in C# and launched from the dev menu or a command line flag. No scenario picker, no planet, no pawn shuffling, no landing site.

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

## How is this different from the vanilla quicktest?

Vanilla's quicktest button drops you on a random map with random colonists and no way to change either. Every launch is a different colony, so you cannot test the same thing twice.

Quickstarts gives you a picker on that button instead, listing every quickstart the loaded mods define. Pick one and you are on the map, paused, with the colony already configured.

Fix the world seed and two runs give you the same planet, landing tile, and colonists. A failure replays instead of vanishing.

Every non-abstract subclass turns up automatically. Adding a quickstart means writing a class and nothing else.

## What can you set up before the map loads?

Hooks run at the points that matter: before generation, after the world exists, after the scenario finishes its own setup, and once the colonists spawn. Set pawn counts, finish research, hand out gear, or open the panel you are working on.

## Can it run as a smoke test in CI?

Yes, and that is the point of writing one. Run the same scenario with a verify flag and the game asserts against the live simulation, then exits with a pass or fail code. It writes both a JSON report and JUnit XML, so CI can annotate a failed assertion on the pull request. No display needed.

Red errors in the game log fail the run by default, which is what catches a broken mod interaction. A wall-clock timeout stops a wedged run and names the stage it died in.

## Requirements

RimWorld 1.6, dev mode on, and one patching library. Harmony or Concord both work. Quickstarts prefers Concord when you have both.

## Getting started

Reference the RimWorks.Quickstarts.Ref package, subclass AbstractQuickstart, and your quickstart shows up in the picker. To ship one from a mod without forcing this dependency on your players, put it in a folder gated behind loadFolders.xml.

Source and documentation: https://github.com/RimWorks/Rimworld-Quickstarts

## More modding tools from RimWorks

- [Pickle](https://steamcommunity.com/sharedfiles/filedetails/?id=3791648678): run Gherkin tests against a live RimWorld session.
- [RimLogging](https://steamcommunity.com/sharedfiles/filedetails/?id=3733484696): structured log viewer and one-click bug report sharing.
- [RimObs](https://steamcommunity.com/sharedfiles/filedetails/?id=3733585062): performance profiler that finds which mod is eating your TPS.

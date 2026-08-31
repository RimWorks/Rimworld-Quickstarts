# Quickstarts

A developer tool for RimWorld. Write a boot-into-game scenario in C#, then launch it from the
dev menu or the command line. The same scenario doubles as a smoke test: run it with a flag and
the game asserts, writes a JSON report, and exits with a pass or fail code.

Requires Harmony or Concord. If both are active, Concord is used.

## Install

Subscribe on the Workshop, or drop a release zip into your `Mods` folder. Quickstarts only does
anything when RimWorld's dev mode is on.

## Define a quickstart

Add a reference to `RimWorks.Quickstarts.Ref`, then subclass `AbstractQuickstart`. Every
non-abstract subclass with a parameterless constructor is found automatically.

```csharp
using RimWorks.Quickstarts;
using RimWorks.Quickstarts.Verification;
using RimWorld;
using Verse;

public class TinyColony : AbstractQuickstart {
  public override TaggedString description => "One small map, three colonists, paused.";

  public override int mapSize => 50;

  public override void PrepareColonists(List<Pawn> pawns) {
    foreach (Pawn pawn in pawns) {
      pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
    }
  }

  public override QuickstartVerification Verify() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("colonists spawned", () => Find.CurrentMap.mapPawns.FreeColonistsSpawnedCount > 0);
    return verification;
  }
}
```

The hooks run in this order:

| Hook | When it runs |
| --- | --- |
| `PostStart` | Before generation, while the menu is still up. Good for dev toggles. |
| `PostApplyConfiguration` | World exists, pawns not generated yet. Good for pawn counts. |
| `PostConfigured` | After the scenario finishes its own setup. Nothing can undo you here. |
| `PrepareColonists` | Map is live, colonists are spawned. |
| `PostLoaded` | Last, just before the pause. |

## Launch one

Three ways, in the order they win:

1. `-quickstart=TinyColony` on the command line.
2. The `RIMWORLD_QUICKSTART` environment variable.
3. The default set in the mod's settings.

With none of those, the game starts normally and the main menu's dev quicktest button opens a
picker instead.

## CI mode

Add `-quickstartverify` to run `Verify()` and exit 0 or 1. Add `-quickstartreport=<path>` to
also write a JSON report, which turns on verify by itself.

```bash
scripts/run-quickstart.sh TinyColony /tmp/report.json
```

The report looks like this:

```json
{
  "quickstart": "TinyColony",
  "passed": true,
  "total": 1,
  "failed": 0,
  "results": [
    { "label": "colonists spawned", "passed": true, "detail": null }
  ]
}
```

## Build

```bash
dotnet build Quickstarts.slnx -c Release
```

Output lands in `Assemblies/`, `Harmony/Assemblies/` and `Concord/Assemblies/`. `loadFolders.xml`
loads only the backend folder whose library is active, so the other one never has to resolve.

## License

MIT.

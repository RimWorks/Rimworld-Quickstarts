# Quickstarts

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=RimWorks_Rimworld-Quickstarts&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=RimWorks_Rimworld-Quickstarts)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=RimWorks_Rimworld-Quickstarts&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=RimWorks_Rimworld-Quickstarts)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=RimWorks_Rimworld-Quickstarts&metric=coverage)](https://sonarcloud.io/summary/new_code?id=RimWorks_Rimworld-Quickstarts)

<img src="https://raw.githubusercontent.com/RimWorks/Rimworld-Quickstarts/main/About/ModIcon.png" alt="Quickstarts icon" width="96" align="right">

A developer tool for RimWorld. Write a boot-into-game scenario in C#, then launch it from the
dev menu or the command line. The same scenario doubles as a smoke test: run it with a flag and
the game asserts, writes a JSON report, and exits with a pass or fail code.

Requires Harmony or Concord. If both are active, Concord is used.

![Quickstarts preview card](https://raw.githubusercontent.com/RimWorks/Rimworld-Quickstarts/main/About/Preview.png)

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

## Fix the world seed

By default every launch generates a new planet, so a CI failure cannot be replayed. Override
`seed` on the quickstart, or pass `-quickstartseed=abc123` to beat whatever the quickstart says.
The seed that ran goes into the log and into the JSON report.

```csharp
public override string? seed => "abc123";
```

Two runs on the same seed give you the same planet, the same landing tile, the same map and the
same colonists. Seeds are only stable within one build of the mod.

## CI mode

Add `-quickstartverify` to run `Verify()` and exit 0 or 1. Add `-quickstartreport=<path>` to
also write a JSON report, which turns on verify by itself.

```bash
scripts/run-quickstart.sh TinyColony /tmp/report.json
```

By default `Verify()` runs against a map nothing has happened on yet. Override `ticksBeforeVerify`
to drive the simulation first, which is where most tick-path bugs surface.

```csharp
public override int ticksBeforeVerify => 2500;   // about 40 in-game seconds
```

A run also fails when the game logs a red error, even if every assertion holds. That is the check
that catches a broken mod interaction. Errors from before the launch, during mod and def loading,
are reported separately and never fail the run.

```csharp
public override bool failOnLogError => false;                  // turn the check off
public override int allowedLogErrors => 2;                     // tolerate a known-noisy dependency
public override IEnumerable<string> ignoredLogErrors => ["null hediff"];   // substrings, no case
```

An ignored error still shows up in the report. It just does not count against the budget.

The report looks like this:

```json
{
  "quickstart": "TinyColony",
  "seed": "abc123",
  "ticksRun": 2500,
  "passed": true,
  "logErrors": 0,
  "logWarnings": 3,
  "logTruncated": false,
  "preLaunchErrors": 0,
  "total": 1,
  "failed": 0,
  "results": [
    { "label": "colonists spawned", "passed": true, "detail": null }
  ],
  "errors": []
}
```

RimWorld keeps the last 1000 log lines and collapses a message repeated in a row into a `repeats`
count. So `logErrors` is a floor, not an exact total, and `logTruncated` tells you when lines were
dropped.

## Timeouts

`scripts/run-quickstart.sh` wraps the game in `timeout`, so a wedged run cannot hang CI. Set
`QUICKSTART_TIMEOUT` to change the limit from the default 600 seconds.

The script also arms an in-game watchdog 30 seconds inside that limit, with
`-quickstarttimeout=<seconds>`. It fires first and writes a report naming the stage the run died
in, which `timeout` alone cannot tell you.

| Code | Meaning |
| --- | --- |
| 0 | every assertion passed and the log was clean |
| 1 | an assertion failed, or the error budget blew |
| 2 | the in-game watchdog fired. a report exists, with `stage` |
| 124 | `timeout` fired. probably no report |
| 137 | `timeout` needed SIGKILL |

Stages, in order: `configuring`, `generating-world`, `generating-map`, `ticking`, `verifying`.

## JUnit output

`-quickstartjunit=<path>` writes the same run as JUnit XML, which GitHub Actions and most other CI
tools read natively. It turns a failed assertion into an annotation on the PR instead of a line in
a log nobody opens. Independent of the JSON, so a run can write both, either or neither.

```xml
<testsuites tests="4" failures="0" errors="0">
  <testsuite name="ScenarioTestQuickstart" tests="4" failures="0" errors="0">
    <testcase classname="ScenarioTestQuickstart" name="a game exists" />
    <testcase classname="ScenarioTestQuickstart" name="no log errors" />
  </testsuite>
</testsuites>
```

One assertion is one `<testcase>`. The error budget is a synthetic `no log errors` case, captured
errors go in `<system-err>`, and a watchdog timeout is an `<error>`. Publish it with:

```yaml
- uses: mikepenz/action-junit-report@v5
  if: always()
  with:
    report_paths: /tmp/quickstart-junit.xml
```

This does not feed SonarCloud. The .NET scanner wants VSTest TRX or Sonar's generic format, not
JUnit.

## Build

```bash
dotnet build Quickstarts.slnx -c Release
```

Output lands in `Assemblies/`, `Harmony/Assemblies/` and `Concord/Assemblies/`. `loadFolders.xml`
loads only the backend folder whose library is active, so the other one never has to resolve.

## License

MIT.

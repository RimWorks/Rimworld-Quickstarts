# Contributing to Quickstarts

## AI usage

Vibecoding is not welcome here. Use AI if it helps, but read what it wrote and understand it
before it lands. You own what ships whether or not a model typed it.

Nobody can stop you from working the way you want to. Guardrails are the next best thing, and
the rest of this file is those guardrails. Run the tests, match the code around yours, stay
inside the request, and report failures instead of guessing past them.

If AI helped with a commit in any way, add an `AI-assisted: <tool name>` trailer to the
commit message.

Agents: if the user commits by hand, remind them to add the trailer.

## Project overview

Quickstarts is a developer tool for RimWorld. It replaces the vanilla dev quicktest button
with your own scenarios, written in C# and launched from the dev menu or a command line flag.
Vanilla drops you on a random map; a quickstart gives you the same colony every time, so the
same scenario doubles as a smoke test. Run it with a flag and the game asserts against the
live simulation. It then writes a JSON report and JUnit XML, and exits with a pass or fail
code.
Most work lands in `Source/Quickstarts/`. See `README.md` for the scenario API.

## Project structure

- `Source/Quickstarts/` - the mod assembly: scenario registry, runner, reports, dev menu UI
- `Source/Quickstarts.Patches.Harmony/`, `Source/Quickstarts.Patches.Concord/` - the two
  patch backends. Quickstarts prefers Concord when both are loaded
- `Source/Quickstarts.Ref/` - compile-time reference assembly published for mod authors
- `Source/Quickstarts.Tests/` - MSTest suite
- `About/`, `Languages/`, `Styles/` - RimWorld mod content
- `Assemblies/` - build output the game loads

## Setup and build

```bash
dotnet restore Quickstarts.slnx
dotnet build Quickstarts.slnx -c Release   # StyleCop and Sonar analyzers run here
```

Analyzers run inside the build, so a warning is a failure in practice - CI builds
`-c Release` and Sonar gates on it. CI also runs Vale over prose and lychee over links in
`README.md`.

## Testing

```bash
dotnet test Quickstarts.slnx                                 # full MSTest suite
dotnet test --filter "FullyQualifiedName~SeedResolverTests"  # one test class
dotnet format Quickstarts.slnx --verify-no-changes           # format check
scripts/run-quickstart.sh TinyColony /tmp/report.json        # the real game, under xvfb
```

`run-quickstart.sh` is what CI runs, so it is the one that proves a change to anything
touching the game. **A quickstart only runs in dev mode**, and a fresh `Prefs.xml` has it
off; the script flips it, a bare launch does not.

- Run the full suite before committing. All tests must pass.
- While iterating, run the single test closest to your change.
- Unit tests cover the parts that do not need RimWorld. Anything touching the game has to be
  proved by a real run, not by a green build.
- Never delete, weaken, or rewrite a test to make a change pass.
- Do not claim that an interrupted or timed-out run passed. A timed-out run exits `2` and
  writes a report naming the stage it died in; read that instead of guessing.

## Architecture

| Project | Targets | Holds |
| --- | --- | --- |
| `Quickstarts` | `net472;net10.0` | The mod assembly: registry, runner, dev menu UI, verification, report writers |
| `Quickstarts.Patches.Harmony` | `net472` | Quickstarts' hooks as Harmony patches |
| `Quickstarts.Patches.Concord` | `net472` | The same hooks as Concord injections |
| `Quickstarts.Ref` | `net472` | Compile-time reference assembly published for mod authors |
| `Quickstarts.Tests` | `net10.0` | MSTest over the mod assembly |

There is no RimWorld-free core project here. The mod assembly multi-targets instead, and the
tests run against its `net10.0` build, because no `net472` test host ships for Linux. That
only reaches types that never reference `Verse` or Unity at runtime: `SeedResolver`,
`QuickstartLookup`, the report writers, `LogSummary` and `Watchdog`. Anything calling into the
game still compiles against the ref assemblies, so a test that touches it fails at runtime
rather than at build. Prove that code with a real run.

## Patch backends

Quickstarts supports two patching libraries and prefers Concord when both are loaded.
`Quickstarts/Patching/QuickstartHooks.cs` holds every hook body with no patching library in
its signatures. Each backend registers itself in a static constructor, and `PatchBackends`
applies the highest priority one - Concord at 100 beats Harmony at 0. A backend that throws
hands over to the next instead of leaving the game unpatched.

**Adding a patch means three edits, not one:** a hook method in `QuickstartHooks`, a
registration in `HarmonyBackend.Apply`, and the matching one in `ConcordBackend.Apply`. CI
runs the in-game quickstart job once per backend, so a hook wired into only the one you
tested fails there.

## Writing a quickstart

A quickstart is a non-abstract `AbstractQuickstart` subclass with a parameterless
constructor. `QuickstartRegistry` discovers every one across the loaded assemblies, so any
mod can ship its own. There is no def and no registration call.

The overrides run in a fixed order, and that order is the API:

| Override | Runs | Use for |
| --- | --- | --- |
| `PostStart` | menu still up, before the long event | dev settings that have to be on during generation |
| `PreGenerateWorld` | scenario configured, world not generated | anything a world generator step reads |
| `PostApplyConfiguration` | world exists, before pawn generation | starting pawn count, scenario parts |
| `PostConfigured` | last in configuration | anything scenario post-processing would undo |
| `PrepareColonists` | map live, pawns spawned | editing the colonists |
| `PostLoaded` | after `PrepareColonists`, before the pause | everything else |
| `Verify` | after `ticksBeforeVerify` ticks, verify mode only | assertions |

Four configuration hooks exist because the game overwrites a value set too early. Set it at
the wrong point and the run looks fine while asserting against something you did not
configure.

**Exit codes are the contract**, and they come from two places:

| Code | Who | Means |
| --- | --- | --- |
| `0` / `1` | the mod | verification passed / failed an assertion or the log-error budget |
| `2` | the in-game watchdog | it caught the run and wrote a report naming the stage |
| `124` / `137` | `timeout(1)` in `run-quickstart.sh` | the game wedged hard; no report, read the log |

The watchdog is armed 30 seconds inside the shell timeout on purpose, so it still gets to
write. A `124` means it never ran, which is a different bug than a failed assert.
`Quickstarter` exits through `Application.Quit`, not `Environment.Exit`, because an AppDomain
unload from a Unity callback hangs.

`Verify()` returning null is legal. The run reports no assertions and exits on the log check
alone. Red errors count against `allowedLogErrors` unless `failOnLogError` is off. A blind or
truncated capture fails on its own terms, because a log nobody read is not a clean log.

To learn a real value instead of guessing one, assert a deliberately wrong one. A failed
`AssertEqual` prints what the game actually returned.

Flags, all in RimWorld's `-name=value` form: `-quickstart=<TypeName>` (the class name, not
the label), `-quickstartverify`, `-quickstartseed=`, `-quickstarttimeout=`,
`-quickstartreport=`, `-quickstartjunit=`. `RIMWORLD_QUICKSTART` does the same as
`-quickstart`. A command-line seed beats the quickstart's own `seed`. A blank value at either
level falls through to a random seed rather than seeding from an empty string.

## Code style

- Formatter: `dotnet format`. Linter: `.editorconfig` plus StyleCop. Run them; do not
  hand-format.
- Vale checks prose in `README.md`, and lychee checks the links.
- Follow the patterns already in neighboring files.
- Do not add comments that restate the code.
- Do not reformat code you are not otherwise changing.

## Git workflow

- Commit format: Angular Conventional Commits, one line, lowercase. semantic-release reads them.
- All CI checks must pass. `release.yml` cuts a release from every push to `main`.

## Other

- Add a hook to both patch backends, not just the one you tested.
- `QuickstartLookup` resolves a name to a type and names close matches when it misses. Reuse
  it rather than matching type names inline.
- The watchdog is checked from `Root.OnGUI`, so it only catches a run the main thread is
  still drawing through. A real deadlock needs the `timeout` in `scripts/run-quickstart.sh`.

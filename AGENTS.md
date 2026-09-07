# AGENTS.md

## AI usage

We don't vibecode here. Use AI if it helps, but read what it wrote and understand it before
it lands. You own what ships, whether or not a model typed it.

We can't stop anyone from working the way they want to. We can set guardrails so what lands
is as good as it can be. The rest of this file is those guardrails. Run the tests, match the
code around yours, stay inside the request, and report failures instead of guessing past them.

If AI helped with a commit in any way, add an `AI-assisted: <tool name>` trailer to the
commit message.

Agents: if the user commits by hand, remind them to add the trailer.

## Project overview

Quickstarts is a developer tool for RimWorld. It replaces the vanilla dev quicktest button
with your own scenarios, written in C# and launched from the dev menu or a command line flag.
Vanilla drops you on a random map; a quickstart gives you the same colony every time, so the
same scenario doubles as a smoke test. Run it with a flag and the game asserts against the
live simulation, writes a JSON report and JUnit XML, then exits with a pass or fail code.
Most work lands in `Source/Quickstarts/`. See `README.md` for the scenario API.

## Project structure

- `Source/Quickstarts/` - the mod assembly: scenario registry, runner, reports, dev menu UI
- `Source/Quickstarts.Patches.Harmony/`, `Source/Quickstarts.Patches.Concord/` - the two
  patch backends. Quickstarts prefers Concord when both are loaded
- `Source/Quickstarts.Ref/` - compile-time reference assembly published for mod authors
- `Source/Quickstarts.Tests/` - MSTest suite
- `About/`, `Languages/`, `Styles/` - RimWorld mod content
- `Assemblies/` - build output the game loads

## Setup & build

```bash
dotnet restore Quickstarts.slnx
dotnet build Quickstarts.slnx -c Release   # StyleCop analyzers run here
```

## Testing

```bash
dotnet test Quickstarts.slnx                              # full MSTest suite
dotnet test --filter "FullyQualifiedName~SeedResolverTests"  # one test class
dotnet format Quickstarts.slnx --verify-no-changes        # format check
```

- Run the full suite before committing. All tests must pass.
- While iterating, run the single test closest to your change.
- Unit tests cover the parts that do not need RimWorld. Anything touching the game has to be
  proved by a real run, not by a green build.
- Never delete, weaken, or rewrite a test to make a change pass.
- Do not claim that an interrupted or timed-out run passed.

## Code style

- Formatter: `dotnet format`. Linter: `.editorconfig` plus StyleCop. Run them; do not
  hand-format.
- Vale checks prose in `README.md`, and lychee checks the links.
- Follow the patterns already in neighboring files.
- Do not add comments that restate the code.
- Do not reformat code you are not otherwise changing.

## Git workflow

- Work on `main`. This repo has no feature branches and no pull requests.
- Commit format: Conventional Commits, one line, lowercase. semantic-release reads them.
- Never commit, push, or open a PR unless asked.
- All CI checks must pass. `release.yml` cuts a release from every push to `main`.

## Boundaries

- Do not modify unrelated files or widen scope beyond the request.
- Do not add dependencies without asking.
- Never commit secrets, API keys, or .env files.
- A hook has to be added to both patch backends, not just the one you tested.
- If a command fails, report the failure. Do not guess or present assumptions as confirmed
  results.

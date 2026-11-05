# dotnet-lateapexearlyspeed-jsonschema — Bowtie harness

A standalone [Bowtie](https://github.com/bowtie-json-schema/bowtie) test harness for the
[`LateApexEarlySpeed.Json.Schema`](https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema) NuGet
package. The harness consumes the library exactly as an external user would — via `PackageReference`,
not a project reference. Its source, meta-schemas and repository configuration are self-contained.
The package version under test is pinned in the
[project file](dotnet-lateapexearlyspeed-jsonschema.csproj); the verified results below record the
specific version used for that historical run.

## Standalone repository and official Bowtie onboarding

Copy this directory's tracked files, including hidden configuration files, directly into the new
repository's root. Do not copy `bin/`, `obj/`, IDE caches, virtual environments or generated reports.
While this directory is nested inside the library repository, its `.github/` configuration is inactive;
GitHub only discovers it after it becomes the standalone repository's root `.github/`.

The workflows target **`bowtie-json-schema/dotnet-lateapexearlyspeed-jsonschema`**, with **`main`** as
the default branch. They deliberately do not publish images or reports from another repository.
Builds and fixture checks can still run in a staging repository after copying these files to its root.

The following external steps require Bowtie maintainers or repository administrators:

1. Create or transfer the public repository into the `bowtie-json-schema` organization, using the exact
   name above, and set its default branch to `main`.
2. Enable GitHub Actions and allow the pinned external actions and Bowtie reusable workflows referenced
   below. Organization policies must allow their declared permissions: image publishing needs
   `packages: write`, `id-token: write`, `attestations: write` and `artifact-metadata: write`; reporting
   needs `contents: write` to publish release assets.
3. Push to `main` and wait for **Rebuild Bowtie Image** to succeed. Make the resulting GHCR package
   public and grant this repository write access if a package with the same name already exists.
4. Run **Report** manually once after the first image is available; later reports run every six hours.
5. Add the repository topic **`bowtie-harness`** and coordinate its inclusion with Bowtie maintainers.
   Bowtie discovers harness repositories with this topic in its own organization when building a
   distribution. Already-installed CLI versions do not automatically acquire new implementation lists;
   maintainers may need to publish a new Bowtie release before central report collection discovers it.
6. Confirm the official site's central collection includes this harness's `report` release. A local
   test or a published GHCR image alone does not register an implementation on [bowtie.report](https://bowtie.report).

For Dependabot auto-merge, administrators must enable **Allow auto-merge**, allow merge commits and
permit the workflow's `contents: write` and `pull-requests: write` permissions. Configure the harness
checks as required branch checks. Auto-merge does not bypass required reviews or other branch rules.

The image address is:

```text
ghcr.io/bowtie-json-schema/dotnet-lateapexearlyspeed-jsonschema
```

### CI, publishing and updates

- [build.yml](.github/workflows/build.yml) builds the .NET project and runs all 19 fixture checks.
  It then calls Bowtie's pinned
  [Harness CI](https://github.com/bowtie-json-schema/bowtie/blob/94e106ed72e2f75700ec19b7927c46cf8417fb8a/.github/workflows/harness-ci.yml)
  to build `amd64` and `arm64` images and run smoke tests. Only a successful **push to `main` in the
  official repository** publishes the image, tagged with the commit SHA, reported library version and
  `latest`. PRs, other branches and manual build runs do not publish. No smoke-test failures are ignored.
  After `check` and `ci` succeed, same-repository Dependabot PRs targeting `main` are eligible for
  `gh pr merge --auto --merge`, except PRs labelled `github_actions`. Ordinary contributor PRs and forks
  are not auto-merged.
- [report.yml](.github/workflows/report.yml) calls Bowtie's pinned
  [Harness Report](https://github.com/bowtie-json-schema/bowtie/blob/94e106ed72e2f75700ec19b7927c46cf8417fb8a/.github/workflows/harness-report.yml)
  at `00:20`, `06:20`, `12:20` and `18:20` UTC, or manually. It tests the published image and updates the
  rolling `report` release. Bowtie's central workflow collects these reports separately. Maintainers
  can stagger the scheduled minute to match their rollout.
- [dependabot.yml](.github/dependabot.yml) checks Docker images daily and Actions weekly, with a
  three-day cooldown. NuGet update automation is intentionally disabled. Update the library's
  PackageReference in [the project file](dotnet-lateapexearlyspeed-jsonschema.csproj) manually through
  a reviewed PR; publishing a new NuGet library version alone does not update the harness.

Image publication remains restricted to `main` pushes; there is no separate publishing workflow for
unmerged Dependabot branches. A merge performed using `GITHUB_TOKEN` does not trigger the normal
push workflow. After such a merge, an authorized maintainer must make a normal human-authenticated
push to `main` (for example, a follow-up commit) to build and publish the updated image. Re-running a
previous workflow builds its original commit, not the newly updated `main`.

These workflows use GitHub-hosted runners, NuGet, Microsoft's public container registry, PyPI and GHCR.
They use the repository's `GITHUB_TOKEN`; no custom PAT, NuGet API key, Docker Hub account, self-hosted
runner or checked-in Bowtie/test-suite source is required. Keep the reusable workflow SHA pins updated
through reviewed Dependabot PRs. Historical-version rebuild/release automation is not configured.

## Prerequisites

- .NET 8 SDK (the commands use the `.csproj`, so `.slnx` support is not required)
- Docker (or Podman) for the container build
- Python 3.13+ for Bowtie CLI and the optional report analysis script
- Bowtie CLI, installed into a local Python venv:

  ```powershell
  # from the harness directory / standalone repository root
  python -m venv .venv
  .venv\Scripts\python.exe -m pip install bowtie-json-schema
  ```

  All commands below run from the harness directory / standalone repository root and assume `$bowtie`
  and `$python` point at the venv's executables:

  ```powershell
  $bowtie = (Resolve-Path .venv\Scripts\bowtie.exe).Path
  $python = (Resolve-Path .venv\Scripts\python.exe).Path
  ```

  While working inside the original library repository, you can instead point both variables at its
  existing parent `.venv`; no new virtual environment is necessary.

## 1. Build

```powershell
dotnet build dotnet-lateapexearlyspeed-jsonschema.csproj --configuration Release
```

## 2. Fast local iteration (no Docker)

Use these whenever `Program.cs` changes, to catch obvious regressions in seconds before paying for a
Docker rebuild + full `bowtie suite` run. The shared local/CI check verifies response counts, sequence
IDs, exact result shapes and all expected Boolean validity values; errors and skips fail the check:

```powershell
.\test-fixtures.ps1
```

For raw IHOP (start/dialect/run/stop) responses from an individual transcript:

```powershell
dotnet run --project dotnet-lateapexearlyspeed-jsonschema.csproj --no-build -c Release -- format-test-cases.jsonl
```

| Fixture | Exercises | Run after touching |
|---|---|---|
| `metaschema-test-cases.jsonl` | `MetaSchemaPopulator` / `$ref` resolution to a dialect's own canonical meta-schema (all 3 dialects) | `MetaSchemaPopulator`, `meta-schemas/*.json`, csproj `CopyToOutputDirectory` wiring |
| `vocabulary-test-cases.jsonl` | Custom `$vocabulary` overrides from the case's registry: disabled validation vocabulary and an unknown optional vocabulary | Custom meta-schema registration |
| `format-test-cases.jsonl` | Per-dialect format defaults (draft7 vs 2019-09/2020-12) and custom format-assertion vocabulary | Custom meta-schema registration / `ValidateFormat` logic |

**Expected output** (the `valid` values in each `run` response, in order):

| Fixture | Expected results by `seq` |
|---|---|
| `metaschema-test-cases.jsonl` | 1, 2, 3: `[true, false]` each |
| `vocabulary-test-cases.jsonl` | 1: `[false, true, true]`; 2: `[false, true]` |
| `format-test-cases.jsonl` | 1: `[true, false]`; 2, 3: `[true, true]`; 4: `[true, false]` |

No fixture should return `skipped` or `errored`. These transcripts include the custom meta-schemas in
`registry`, just as real Bowtie input does; expected validity is not sent to the harness.

### PowerShell output-encoding gotcha

Native PowerShell redirection (`>`, `1>`, `2>`) defaults to **UTF-16**, which silently corrupts JSON-Lines
output and breaks any downstream JSON parsing. Always pipe through `Out-File -Encoding utf8` instead:

```powershell
# BAD - produces UTF-16, breaks JSON parsing:
... > output.json

# GOOD:
... | Out-File -FilePath output.json -Encoding utf8
```

## 3. Docker build

```powershell
docker build -t local/lateapexearlyspeed-jsonschema .
```

## 4. Bowtie smoke test

```powershell
& $bowtie smoke -i image:local/lateapexearlyspeed-jsonschema -f json | Out-File -FilePath smoke-check.log -Encoding utf8
```

`-f json` is required for the locally verified Bowtie version — its own `pretty` renderer (the default
when attached to a terminal) has an upstream bug (`bowtie/_smoke.py`) that crashes with
`AttributeError: 'SkippedTest' object has no attribute 'valid'` whenever any result is `skipped`. This is a
Bowtie bug, not a harness bug — `-f json` bypasses the broken rich-console path entirely and gives clean
output.

Expected: `"success": true` for all 3 dialects.

## 5. Bowtie suite (official compliance scope, all 3 dialects)

**Never merge stdout/stderr with `2>&1`** when capturing suite output — Bowtie writes progress/log text to
stderr, and merging streams interleaves non-JSON lines into the JSONL result file, corrupting it. Always
redirect them separately:

For local suite runs only, clone the official test suite into the ignored directory below (once), or
point `$suite` at an existing checkout. CI reporting obtains the suite itself; it does not need this clone.

```powershell
git clone https://github.com/json-schema-org/JSON-Schema-Test-Suite.git JSON-Schema-Test-Suite
$suite = (Resolve-Path JSON-Schema-Test-Suite\tests).Path
# Original library repository checkout, if already available:
# $suite = (Resolve-Path ..\JSON-Schema-Test-Suite\tests).Path

& $bowtie suite -V -i image:local/lateapexearlyspeed-jsonschema "$suite\draft7" `
    2>suite-draft7.err | Out-File -FilePath suite-draft7.json -Encoding utf8

& $bowtie suite -V -i image:local/lateapexearlyspeed-jsonschema "$suite\draft2019-09" `
    2>suite-draft2019-09.err | Out-File -FilePath suite-draft2019-09.json -Encoding utf8

& $bowtie suite -V -i image:local/lateapexearlyspeed-jsonschema "$suite\draft2020-12" `
    2>suite-draft2020-12.err | Out-File -FilePath suite-draft2020-12.json -Encoding utf8
```

> Note: `bowtie suite`/`bowtie site collect` only scan each dialect's **top-level** `.json` files
> (`Path.iterdir()`, non-recursive — confirmed in `bowtie/_suite.py`). The `optional/` subfolder is excluded from
> this scope by design across the whole Bowtie ecosystem (including the published bowtie.report compliance
> site), not something specific to this invocation.

Then summarize:

```powershell
Get-Content suite-draft7.json       | & $bowtie summary --show failures | Out-File summary-draft7.txt -Encoding utf8
Get-Content suite-draft2019-09.json | & $bowtie summary --show failures | Out-File summary-draft2019-09.txt -Encoding utf8
Get-Content suite-draft2020-12.json | & $bowtie summary --show failures | Out-File summary-draft2020-12.txt -Encoding utf8
```

Or use the bundled `analyze.py` script for a more detailed per-case/per-test breakdown (handles both
whole-case and per-test `skipped`/`errored` entries, which `bowtie summary` only totals):

```powershell
& $python analyze.py `
    suite-draft7.json suite-draft2019-09.json suite-draft2020-12.json
```

**Expected result**: no skipped cases or tests. The `vocabulary.json` cases for 2019-09 and 2020-12
must validate normally, not be skipped. Unsupported Unicode regex syntax is executed and reported
honestly as errors or validity mismatches, not filtered out.

## Vocabulary and format configuration

Custom meta-schemas containing `$vocabulary` are registered from the case's `registry` in
`JsonValidatorOptions.CustomMetaSchemaRegistry` **before** constructing the validator. Ordinary
external `$ref` resolution still uses `MetaSchemaPopulator` and the case's registry.

For standard dialects, `format` remains an assertion in draft7 and annotation-only in 2019-09/2020-12.
For custom meta-schemas, format validation is enabled so the library can honor the selected vocabulary:
`format-annotation` remains annotation-only, while `format-assertion` validates even when its
`$vocabulary` value is `false` (that value controls whether support is required, not assertion behavior).

## Known regex limitation

The current suite includes ECMAScript Unicode property escapes such as `\p{Letter}` in 2020-12
`pattern.json` and `patternProperties.json`. .NET's regex engine does not recognize this property name.
The harness does not translate these patterns or skip their tests: exceptions are returned through
the normal IHOP `errored` response, with the original exception in `context.message`.

## Verified results

Verified on 2026-11-04 with NuGet **4.2.3**, Bowtie **2026.7.4**, and test-suite revision
`245a45abc90190c0bff04d97e6d2b541b4bb9f12` (top-level official compliance scope).
The Docker image was built using cached official .NET 8 SDK/runtime image digests; its runtime was
8.0.32. All suite runs used `-V` to validate the harness's IHOP requests and responses.

| Dialect | Cases | Tests | Passed | Errored cases (affected tests) | Skipped | Validity mismatches |
|---|---:|---:|---:|---:|---:|---:|
| draft7 | 259 | 927 | 927 | 0 | 0 | 0 |
| draft2019-09 | 374 | 1259 | 1259 | 0 | 0 | 0 |
| draft2020-12 | 385 | 1300 | 1294 | 2 (6) | 0 | 0 |

The only errored cases were:

- `pattern with Unicode property escape requires unicode mode`
- `patternProperties with Unicode property escape`

Both use `^\p{Letter}+$` and report `RegexParseException: Unknown property 'Letter'`.
They are case-level errors, not `valid: false` responses or skips.

All 10 tests from the two dialects' `vocabulary.json` files passed. Report completeness and exact
per-case result lengths were checked against the local suite, including confirmation that execution
did not stop early. Bowtie smoke passed for all three dialects, and all 19 local fixture checks passed.
As a focused supplemental check outside the table's compliance scope, all four tests in the official
optional `format-assertion.json` passed via `bowtie run -D draft2020-12 -V` with their remote meta-schemas
in the registry.

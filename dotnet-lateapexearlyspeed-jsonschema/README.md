# dotnet-lateapexearlyspeed-jsonschema — Bowtie harness

A standalone [Bowtie](https://github.com/bowtie-json-schema/bowtie) test harness for the
[`LateApexEarlySpeed.Json.Schema`](https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema) NuGet
package. This folder is self-contained (own `.slnx`, not part of the repo's root `.sln`) and consumes the
library exactly as an external user would — via `PackageReference`, not a project reference.
The harness currently targets NuGet package version **4.2.3**.

## Prerequisites

- .NET 8 SDK
- Docker (or Podman) for the container build
- Bowtie CLI, installed into this repo's Python venv:

  ```powershell
  # from repo root
  .venv\Scripts\python.exe -m pip install bowtie-json-schema
  ```

  All commands below assume `$bowtie` points at the venv's executable:

  ```powershell
  $bowtie = (Resolve-Path .venv\Scripts\bowtie.exe).Path
  ```

## 1. Build

```powershell
cd dotnet-lateapexearlyspeed-jsonschema
dotnet build dotnet-lateapexearlyspeed-jsonschema.slnx
```

## 2. Fast local iteration (no Docker)

Use these whenever `Program.cs` changes, to catch obvious regressions in seconds before paying for a
Docker rebuild + full `bowtie suite` run. Each pipes a small hand-crafted IHOP (start/dialect/run/stop)
transcript directly into the console app:

```powershell
Get-Content metaschema-test-cases.jsonl | dotnet run --project dotnet-lateapexearlyspeed-jsonschema.csproj --no-build -c Debug
Get-Content vocabulary-test-cases.jsonl  | dotnet run --project dotnet-lateapexearlyspeed-jsonschema.csproj --no-build -c Debug
Get-Content format-test-cases.jsonl      | dotnet run --project dotnet-lateapexearlyspeed-jsonschema.csproj --no-build -c Debug
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

`-f json` is required — Bowtie's own `pretty` renderer (the default when attached to a terminal) has an
upstream bug ([`_smoke.py`](../.venv/Lib/site-packages/bowtie/_smoke.py)) that crashes with
`AttributeError: 'SkippedTest' object has no attribute 'valid'` whenever any result is `skipped`. This is a
Bowtie bug, not a harness bug — `-f json` bypasses the broken rich-console path entirely and gives clean
output.

Expected: `"success": true` for all 3 dialects.

## 5. Bowtie suite (official compliance scope, all 3 dialects)

**Never merge stdout/stderr with `2>&1`** when capturing suite output — Bowtie writes progress/log text to
stderr, and merging streams interleaves non-JSON lines into the JSONL result file, corrupting it. Always
redirect them separately:

```powershell
cd ..  # repo root - JSON-Schema-Test-Suite/ path below is relative to here

& $bowtie suite -V -i image:local/lateapexearlyspeed-jsonschema JSON-Schema-Test-Suite\tests\draft7 `
    2>dotnet-lateapexearlyspeed-jsonschema\suite-draft7.err `
    | Out-File -FilePath dotnet-lateapexearlyspeed-jsonschema\suite-draft7.json -Encoding utf8

& $bowtie suite -V -i image:local/lateapexearlyspeed-jsonschema JSON-Schema-Test-Suite\tests\draft2019-09 `
    2>dotnet-lateapexearlyspeed-jsonschema\suite-draft2019-09.err `
    | Out-File -FilePath dotnet-lateapexearlyspeed-jsonschema\suite-draft2019-09.json -Encoding utf8

& $bowtie suite -V -i image:local/lateapexearlyspeed-jsonschema JSON-Schema-Test-Suite\tests\draft2020-12 `
    2>dotnet-lateapexearlyspeed-jsonschema\suite-draft2020-12.err `
    | Out-File -FilePath dotnet-lateapexearlyspeed-jsonschema\suite-draft2020-12.json -Encoding utf8
```

> Note: `bowtie suite`/`bowtie site collect` only scan each dialect's **top-level** `.json` files
> (`Path.iterdir()`, non-recursive — confirmed in
> [`_suite.py`](../.venv/Lib/site-packages/bowtie/_suite.py)). The `optional/` subfolder is excluded from
> this scope by design across the whole Bowtie ecosystem (including the published bowtie.report compliance
> site), not something specific to this invocation.

Then summarize:

```powershell
cd dotnet-lateapexearlyspeed-jsonschema
Get-Content suite-draft7.json       | & $bowtie summary --show failures | Out-File summary-draft7.txt -Encoding utf8
Get-Content suite-draft2019-09.json | & $bowtie summary --show failures | Out-File summary-draft2019-09.txt -Encoding utf8
Get-Content suite-draft2020-12.json | & $bowtie summary --show failures | Out-File summary-draft2020-12.txt -Encoding utf8
```

Or use the bundled `analyze.py` script for a more detailed per-case/per-test breakdown (handles both
whole-case and per-test `skipped`/`errored` entries, which `bowtie summary` only totals):

```powershell
..\.venv\Scripts\python.exe analyze.py `
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

# dotnet-lateapexearlyspeed-jsonschema — Bowtie harness

A standalone [Bowtie](https://github.com/bowtie-json-schema/bowtie) test harness for the
[`LateApexEarlySpeed.Json.Schema`](https://www.nuget.org/packages/LateApexEarlySpeed.Json.Schema) NuGet
package. This folder is self-contained (own `.slnx`, not part of the repo's root `.sln`) and consumes the
library exactly as an external user would — via `PackageReference`, not a project reference.

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
  $bowtie = "c:/Users/PengHe/source/repos/Lateapexearlyspeed.JsonSchema/.venv/Scripts/bowtie.exe"
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
| `vocabulary-test-cases.jsonl` | The `usesCustomMetaschema` skip path (custom `$vocabulary` overrides) | The `usesCustomMetaschema` condition / skip message |
| `format-test-cases.jsonl` | Per-dialect `ValidateFormat` default (draft7 vs 2019-09/2020-12) + custom-metaschema format-assertion skip | `validateOptions` / `ValidateFormat` logic |

**Expected output** (manually eyeball `"valid"`/`"skipped"` values against each fixture's comments/intent —
there's no automated assertion here, this is just a quick sanity signal).

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

Expected: `"success": true` for all 3 dialects — there are no remaining documented smoke-test gaps (see
"Known, documented gaps" below).

## 5. Bowtie suite (official compliance scope, all 3 dialects)

**Never merge stdout/stderr with `2>&1`** when capturing suite output — Bowtie writes progress/log text to
stderr, and merging streams interleaves non-JSON lines into the JSONL result file, corrupting it. Always
redirect them separately:

```powershell
cd ..  # repo root - JSON-Schema-Test-Suite/ path below is relative to here

& $bowtie suite -i image:local/lateapexearlyspeed-jsonschema JSON-Schema-Test-Suite/tests/draft7 `
    2>dotnet-lateapexearlyspeed-jsonschema/suite-draft7.err `
    | Out-File -FilePath dotnet-lateapexearlyspeed-jsonschema/suite-draft7.json -Encoding utf8

& $bowtie suite -i image:local/lateapexearlyspeed-jsonschema JSON-Schema-Test-Suite/tests/draft2019-09 `
    2>dotnet-lateapexearlyspeed-jsonschema/suite-draft2019-09.err `
    | Out-File -FilePath dotnet-lateapexearlyspeed-jsonschema/suite-draft2019-09.json -Encoding utf8

& $bowtie suite -i image:local/lateapexearlyspeed-jsonschema JSON-Schema-Test-Suite/tests/draft2020-12 `
    2>dotnet-lateapexearlyspeed-jsonschema/suite-draft2020-12.err `
    | Out-File -FilePath dotnet-lateapexearlyspeed-jsonschema/suite-draft2020-12.json -Encoding utf8
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
c:/Users/PengHe/source/repos/Lateapexearlyspeed.JsonSchema/.venv/Scripts/python.exe analyze.py `
    suite-draft7.json suite-draft2019-09.json suite-draft2020-12.json
```

**Expected result**: 0 errors, 0 failures for all 3 dialects; only the documented `vocabulary.json` skips
(5 tests each, 2019-09 and 2020-12 — see below; none for draft7, which predates `$vocabulary`).

## Known, documented gaps

A genuine upstream library limitation (not a harness bug), reported to Bowtie as `skipped` with an
`issue_url` pointing at the library's issue tracker:

1. **Custom `$vocabulary` declarations** (official suite's root-level `vocabulary.json`, 2019-09/2020-12
   only — 2 cases / 5 tests each) — the library has no mechanism to read a schema's own custom `$schema`
   and adjust keyword semantics accordingly (e.g. disabling the validation vocabulary entirely). Detected
   structurally (schema's `$schema` points outside the 3 known dialect URIs) rather than by filename, since
   the IHOP protocol never tells the harness which file a case came from — this keeps the skip correct
   automatically even if the suite's `vocabulary.json` content changes in the future.

This skip response is applied to **every test in the affected case** (Bowtie's `TestCase.without_expected_results()`
strips the expected `"valid"` field before sending cases to any harness, so the decision must be made purely
from the schema itself, up front — never by comparing against an expected result the harness is never given).

> `$schema` nested in a non-resource-root subschema (no accompanying `$id`) used to be an additional
> documented gap here (triggered by Bowtie's own hardcoded `bottom_schema` construct used internally by
> `bowtie smoke`, see [`_core.py`](../.venv/Lib/site-packages/bowtie/_core.py)) — fixed upstream in
> `LateApexEarlySpeed.Json.Schema` 4.2.2, which now accepts and ignores it instead of throwing.

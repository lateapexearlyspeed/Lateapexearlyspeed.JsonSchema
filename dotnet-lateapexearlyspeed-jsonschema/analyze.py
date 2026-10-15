import json
import sys

def analyze(path):
    cases = {}
    outcomes = {}
    for line in open(path, encoding="utf-8-sig"):
        line = line.strip()
        if not line:
            continue
        obj = json.loads(line)
        if "case" in obj:
            cases[obj["seq"]] = obj["case"]
        elif "results" in obj or "errored" in obj or "skipped" in obj:
            outcomes[obj["seq"]] = obj

    failures = []
    errors = []
    skips = []
    test_skips = []
    for seq, case in cases.items():
        outcome = outcomes.get(seq)
        if outcome is None:
            continue
        if outcome.get("errored"):
            errors.append((seq, case, outcome))
        elif outcome.get("skipped"):
            skips.append((seq, case, outcome))
        else:
            results = outcome["results"]
            for test, result in zip(case["tests"], results):
                if result.get("skipped"):
                    test_skips.append((seq, case["description"], test["description"], result.get("message")))
                elif result.get("errored"):
                    errors.append((seq, case, result))
                elif result["valid"] != test["valid"]:
                    failures.append((seq, case["description"], test["description"], test["valid"], result["valid"]))

    print(f"=== {path} ===")
    print(f"whole-case errors: {len(errors)}, whole-case skips: {len(skips)}, per-test skips: {len(test_skips)}, failures: {len(failures)}")
    print()
    for seq, case, outcome in errors:
        print(f"[ERRORED] seq={seq} case={case['description'] if isinstance(case, dict) else case!r}")
        print("  message:", outcome["context"]["message"].splitlines()[0])
    print()
    for seq, case, outcome in skips:
        print(f"[SKIPPED-CASE] seq={seq} case={case['description']!r}: {outcome.get('message')}")
    print()
    for seq, case_desc, test_desc, message in test_skips:
        print(f"[SKIPPED-TEST] seq={seq} case={case_desc!r} test={test_desc!r}: {message}")
    print()
    for seq, case_desc, test_desc, expected, actual in failures:
        print(f"[FAILED] seq={seq} case={case_desc!r} test={test_desc!r} expected={expected} actual={actual}")

if __name__ == "__main__":
    for p in sys.argv[1:]:
        analyze(p)
        print()

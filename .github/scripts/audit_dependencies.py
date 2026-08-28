#!/usr/bin/env python3
"""Turn `dotnet list package --vulnerable --format json` reports into one summary.

A project with no vulnerable packages is emitted without a "frameworks" key, so its
absence means clean rather than unevaluated.

Exit codes: 0 clean, 8 vulnerable packages found, 1 the audit itself failed.

8 avoids colliding with the interpreter's own exit codes, so a missing or unreadable
script cannot be mistaken for a finding.
"""
import json
import sys

EXIT_CLEAN = 0
EXIT_FAILED = 1
EXIT_FOUND = 8


def fail(message):
    print(f"dependency audit failed: {message}", file=sys.stderr)
    sys.exit(EXIT_FAILED)


def main(paths):
    if not paths:
        fail("usage: audit_dependencies.py <report.json> [<report.json> ...]")

    findings = {}

    for path in paths:
        try:
            with open(path) as handle:
                data = json.load(handle)
        except (OSError, ValueError) as error:
            fail(f"could not read {path}: {error}")

        version = data.get("version")
        if version != 1:
            fail(f"{path} uses report schema version {version!r}, expected 1")

        problems = data.get("problems") or []
        if problems:
            fail(f"{path} reported problems: {problems}")

        projects = data.get("projects")
        if not projects:
            fail(f"{path} listed no projects, so nothing was actually audited")

        for project in projects:
            project_path = project.get("path")
            if not project_path:
                fail(f"{path} contains a project entry without a path")
            project_name = project_path.rsplit("/", 1)[-1]

            for framework in project.get("frameworks") or []:
                for kind in ("topLevelPackages", "transitivePackages"):
                    for package in framework.get(kind) or []:
                        for vulnerability in package.get("vulnerabilities") or []:
                            key = (
                                project_name,
                                package["id"],
                                package["resolvedVersion"],
                                vulnerability["advisoryurl"],
                            )
                            findings[key] = vulnerability["severity"]

    if not findings:
        print("No vulnerable packages.")
        return EXIT_CLEAN

    print("| Project | Package | Resolved | Severity | Advisory |")
    print("| --- | --- | --- | --- | --- |")
    for (project_name, package_id, resolved, url), severity in sorted(findings.items()):
        print(f"| `{project_name}` | `{package_id}` | {resolved} | {severity} | {url} |")
    return EXIT_FOUND


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

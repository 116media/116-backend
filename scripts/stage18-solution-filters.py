#!/usr/bin/env python3
"""Generate the solution filters from the solution itself (stage 18: 18.11).

Usage: scripts/stage18-solution-filters.py [--solution 116_backend.refactor.sln]

One filter per module, for working on a module without loading the other three, and one per test category,
which is what CI runs instead of naming suite directories it would have to edit for every new module.
"""

from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
MODULES = ("Content", "Identity", "Storage", "Mailer")


def projects(solution: Path) -> list[str]:
    out = subprocess.run(
        ["dotnet", "sln", str(solution), "list"], cwd=REPO, capture_output=True, text=True, check=True
    ).stdout
    return sorted(line.strip() for line in out.splitlines() if line.strip().endswith(".csproj"))


def write(path: Path, solution: Path, selected: list[str]) -> None:
    # `solution.path` is relative to this filter; the project paths are relative to the solution itself.
    relative_solution = Path(*[".."] * len(path.parent.relative_to(REPO).parts)) / solution.name
    document = {"solution": {"path": relative_solution.as_posix(), "projects": selected}}
    path.write_text(json.dumps(document, indent=4) + "\n", encoding="utf-8")
    print(f"  {path.relative_to(REPO)}: {len(selected)} projects")


def main(argv: list[str]) -> int:
    name = argv[argv.index("--solution") + 1] if "--solution" in argv else "116_backend.refactor.sln"
    solution = REPO / name
    all_projects = projects(solution)
    root = "src.refactor" if "refactor" in name else "src"
    tests_root = "tests.refactor" if "refactor" in name else "tests"

    for module in MODULES:
        # A module, its published seam, the shared kernel it stands on, and the host that composes it.
        selected = [
            p
            for p in all_projects
            if f"/modules/{module}/" in p or "/shared/" in p or "/host/" in p or p.startswith(f"{tests_root}/")
        ]
        write(REPO / root / "modules" / module / f"{module}.slnf", solution, selected)

    for category, match in (("unit", ".Unit.Tests.csproj"), ("integration", (".Integration.Tests.csproj", "EndToEnd.Tests.csproj"))):
        wanted = (match,) if isinstance(match, str) else match
        selected = [p for p in all_projects if p.endswith(wanted)]
        write(REPO / tests_root / f"{category}.slnf", solution, selected)

    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

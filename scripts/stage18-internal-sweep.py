#!/usr/bin/env python3
"""Make every type nothing outside its own project names `internal` (stage 18 D5).

Usage: scripts/stage18-internal-sweep.py [--dry-run]

The candidate set is computed, not listed: a public top-level type qualifies only when no file in any
other project — the host, another module, a test suite — mentions its name. That is why no
`InternalsVisibleTo` follows from this sweep; anything a test names stays public.

Reflection-based discovery still reaches internal types: Carter scans `GetTypes()`, FluentValidation is
registered with `includeInternalTypes: true`, and EF's `ApplyConfigurationsFromAssembly` uses
`GetConstructibleTypes()`. Scrutor is the exception and is opted in with `publicOnly: false`.
"""

from __future__ import annotations

import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]

IDENTIFIER_RX = re.compile(r"\b\w+\b")
# Both directions, so the sweep computes accessibility from scratch every run rather than only tightening it:
# hand-authored files are not restored by the copy, so a decision made on an earlier run has to be revisable.
DECLARATION_RX = re.compile(
    r"^(public|internal) ((?:sealed |static |abstract |partial |readonly )*(?:class|interface|record(?: struct| class)?|struct|enum)\s+(\w+))",
    re.M,
)

# Types a scanner or the host names by reflection through a string, where the analysis cannot see the use.
KEEP_PUBLIC = {"Program"}

EXTENSION_METHOD_RX = re.compile(r"\(\s*this [\w<>,.?\[\]]+ \w+")

# Contracts whose implementations a framework instantiates by reflection over public types only. EF's
# ApplyConfigurationsFromAssembly is the measured case: an internal configuration is skipped, the model
# silently loses it, and the next migration check reports pending changes.
REFLECTION_INSTANTIATED = ("IEntityTypeConfiguration<",)


def source_files() -> dict[Path, str]:
    files: dict[Path, str] = {}
    for root in ("src.refactor", "tests.refactor"):
        for f in (REPO / root).rglob("*.cs"):
            path = f.as_posix()
            if "/obj/" not in path and "/bin/" not in path:
                files[f] = f.read_text(encoding="utf-8")
    return files


def project_index(files: dict[Path, str]) -> dict[Path, str]:
    project: dict[Path, str] = {}
    for proj in (REPO / "src.refactor").rglob("*.csproj"):
        if "/obj/" in proj.as_posix():
            continue
        for f in proj.parent.rglob("*.cs"):
            if f in files:
                project[f] = proj.stem
    for f in files:
        project.setdefault(f, "tests")
    return project


def own_types(text: str, owner: str) -> set[str]:
    """The top-level types a source file declares, for files that take part in the sweep."""
    if owner.endswith((".Tests", ".TestData")) or owner == "tests":
        return set()
    return {name for _, _, name in DECLARATION_RX.findall(text)}


def main(argv: list[str]) -> int:
    dry_run = "--dry-run" in argv
    files = source_files()
    project = project_index(files)

    mentions: dict[str, set[str]] = defaultdict(set)
    declared: dict[str, set[str]] = defaultdict(set)
    for f, text in files.items():
        owner = project[f]
        for token in set(IDENTIFIER_RX.findall(text)):
            mentions[token].add(owner)
        if owner.endswith((".Tests", ".TestData")) or owner == "tests":
            continue
        # An extension method is called through its receiver, so the class holding it is never named by the
        # projects that depend on it — the analysis would read that as unused.
        holds_extensions = EXTENSION_METHOD_RX.search(text) is not None
        instantiated_by_reflection = any(contract in text for contract in REFLECTION_INSTANTIATED)
        for _, modifiers, name in DECLARATION_RX.findall(text):
            if instantiated_by_reflection:
                continue
            if holds_extensions and "static class" in modifiers:
                continue
            declared[owner].add(name)

    hidden = {
        name
        for owner, names in declared.items()
        for name in names
        if mentions[name] <= {owner} and name not in KEEP_PUBLIC
    }
    total = sum(len(n) for n in declared.values())
    print(f"{total} public types in src/, {len(hidden)} named by no other project")

    # A type is only hideable if nothing still public can expose it: a public signature naming an internal
    # type does not compile. Files whose every type is hidden cannot expose anything, so their mentions are
    # free; any other file keeps whatever it names public, and that ripples until the set stops shrinking.
    sources = {f: (own_types(files[f], project[f]), set(IDENTIFIER_RX.findall(files[f]))) for f in files}
    while True:
        forced = {
            token
            for f, (names, tokens) in sources.items()
            if names and not names <= hidden
            for token in (tokens & hidden) - names
        }
        if not forced:
            break
        hidden -= forced
    print(f"{len(hidden)} survive the accessibility closure ({total - len(hidden)} stay public)")

    changed = 0
    for f, text in files.items():
        if project[f].endswith((".Tests", ".TestData")) or project[f] == "tests":
            continue
        new = DECLARATION_RX.sub(
            lambda m: ("internal " if m.group(3) in hidden else "public ") + m.group(2), text
        )
        if new != text:
            changed += 1
            if not dry_run:
                f.write_text(new, encoding="utf-8")
    print(f"{'would rewrite' if dry_run else 'rewrote'} {changed} files")

    if dry_run:
        return 0

    # Scrutor skips non-public classes unless told otherwise; every other scanner already sees them.
    cqrs = REPO / "src.refactor/shared/src/BuildingBlocks.Presentation/Extensions/CqrsExtension.cs"
    text = cqrs.read_text(encoding="utf-8")
    if "publicOnly: true" in text:
        cqrs.write_text(text.replace("publicOnly: true", "publicOnly: false"), encoding="utf-8")
        print("CqrsExtension: handler scan opted into non-public classes")

    return subprocess.run(["dotnet", "csharpier", "format", "src.refactor"], cwd=REPO, check=False).returncode


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))

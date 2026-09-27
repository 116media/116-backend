#!/usr/bin/env python3
"""Regenerate the tree listings in module-restructure-study/03-full-target-structure.md from the move map.

Usage: scripts/stage18-refresh-study03.py

Every block is the map verbatim: since the owner ruled that all four modules are layered (D1), 03's maximal
shape and stage 18's target are the same tree.
"""

from __future__ import annotations

import csv
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
MAP = REPO / "docs/architecture-audit/implementation-specs/stage-18-move-map.tsv"
DOC = REPO / "docs/architecture-audit/module-restructure-study/03-full-target-structure.md"

# title -> (path prefix to list, the head lines above it, the indent its children start at)
BLOCKS = {
    "src/modules/ · Content/": ("src/modules/Content/", ["├── src/", "│   ├── modules/", "│   │   ├── Content/"], "│   │   │   "),
    "src/modules/Identity/": ("src/modules/Identity/", ["│   │   ├── Identity/"], "│   │   │   "),
    "src/modules/Storage/": ("src/modules/Storage/", ["│   │   ├── Storage/"], "│   │   │   "),
    "src/modules/Mailer/": ("src/modules/Mailer/", ["│   │   └── Mailer/"], "│   │       "),
    "src/shared/ · src/": ("src/shared/src/", ["│   ├── shared/", "│   │   ├── src/"], "│   │   │   "),
    "src/shared/tests/": ("src/shared/tests/", ["│   │   └── tests/"], "│   │       "),
    "src/host/": ("src/host/", ["│   └── host/"], "│       "),
    "tests/": ("tests/", ["├── tests/"], "│   "),
    "root files": (None, [], ""),
}


def root_files() -> list[str]:
    """The tracked files at the repository root; the swap does not move any of them."""
    out = subprocess.run(["git", "ls-files"], cwd=REPO, capture_output=True, text=True, check=True).stdout
    names = sorted(line for line in out.splitlines() if "/" not in line)
    return [f"{'└── ' if i == len(names) - 1 else '├── '}{n}" for i, n in enumerate(names)]


def tree(paths: list[str]) -> dict:
    root: dict = {}
    for p in paths:
        node = root
        for part in p.split("/"):
            node = node.setdefault(part, {})
    return root


def render(node: dict, prefix: str) -> list[str]:
    names = sorted(node, key=lambda n: (not node[n], n.lower()))
    out = []
    for i, name in enumerate(names):
        last = i == len(names) - 1
        out.append(f"{prefix}{'└── ' if last else '├── '}{name}{'/' if node[name] else ''}")
        out += render(node[name], prefix + ("    " if last else "│   "))
    return out


def main() -> int:
    paths = [
        r["new_path"]
        for r in csv.DictReader(MAP.open(), delimiter="\t")
        if r["kind"] in ("move", "new") and r["new_path"]
    ]
    s = DOC.read_text(encoding="utf-8")
    for title, (base, head, indent) in BLOCKS.items():
        if base is None:
            lines = root_files()
        else:
            lines = head + render(tree(sorted(p[len(base):] for p in paths if p.startswith(base) and "/" in p[len(base):] or (p.startswith(base) and base))), indent)
        rx = re.compile(r"(<summary><code>" + re.escape(title) + r"</code> \()\d+( lines\)</summary>\n\n```text\n)(.*?)(\n```)", re.S)
        m = rx.search(s)
        if not m:
            print(f"  ! block not found: {title}")
            continue
        was = len(m.group(3).splitlines())
        s = s[: m.start()] + f"{m.group(1)}{len(lines)}{m.group(2)}" + "\n".join(lines) + m.group(4) + s[m.end() :]
        print(f"  {title}: {was} -> {len(lines)} lines")
    DOC.write_text(s, encoding="utf-8")
    return 0


if __name__ == "__main__":
    sys.exit(main())

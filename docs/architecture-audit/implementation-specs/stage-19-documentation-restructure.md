# Stage 19 — Documentation restructure

Closes **[doc 09]** in full (9.1–9.14) and **[02 §2]**. The docs describe a system that was
never built, contradict each other and the code, and the canonical guide is gitignored. Last
stage by design: written against the post-restructure tree, so it documents what actually
shipped.

> Draft — finalized against the tree Stage 18 lands on.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Entry point | index page, or per-audience guides | **One `docs/README.md` index** with three reading paths (new engineer, operator, reviewer); the five competing "read me first" documents demoted into it or deleted `[9.6]` / `[9.13]`. |
| D2 | Status signalling | prose, or front matter | **Front matter on every doc** — `status:` is machine-checkable, so a CI grep can list `draft`/`stale` docs and the archive step is a field flip, not a rewrite `[9.8]` / `[9.9]` / `[9.11]`. |
| D3 | CLAUDE.md | patch the false facts, or regenerate from the tree | **Regenerate.** The structural sections (tree, schemas, key files, commands, templates) are re-derived from the post-Stage-18 tree and every command/template is executed once before committing `[9.1–9.4]`. The behavioural rules (testing, commits, style) carry over — they were never the false part. |
| D4 | Testing docs | merge the three trees, or crown one | **Crown `docs/testing/00-unit-vs-integration-rules.md`** (already canonical in CLAUDE.md); the other two trees archive with a pointer; the eight contradictions resolve in its favour `[9.7]`. |
| D5 | Load-bearing rules | scatter in guides, or one architecture doc | **`ARCHITECTURE.md` rewritten to the built system** — module boundaries (Stage 14/18 rules), event flow (Stage 13 outbox), request pipeline, the exception/problem vocabulary (Stage 7/16) — each section citing the architecture test or spec that enforces it, so drift is detectable `[9.5]` / `[9.12]` / `[02 §2]`. |

---

## Checklist

- [ ] 19.1 — `docs/README.md` index; competing entry docs resolved
- [ ] 19.2 — Front matter on every doc; CI check listing non-`current` docs
- [ ] 19.3 — CLAUDE.md regenerated, tracked, every command executed once
- [ ] 19.4 — ARCHITECTURE.md describes the built system + enforcement pointers
- [ ] 19.5 — Testing docs collapsed; contradictions resolved
- [ ] 19.6 — Audit spec files stamped with as-built deviations (Stages 1–18)
- [ ] 19.7 — Conventions doc; corrupted/stray docs repaired or deleted; README links resolve `[9.13]` / `[9.14]`
- [ ] 19.8 — Verify: link check green; cold-start walkthrough boots the app from the docs alone

---

## The mechanics

Front matter, on every doc:

```yaml
---
status: current        # current | draft | archived | superseded-by:<path>
owner: backend
last-verified: 2026-09-05
---
```

The CI guard (runs in the docs job):

```bash
# every doc has front matter, and archived docs live under docs/archive/
grep -rL "^status:" docs --include="*.md" | grep -v docs/archive && exit 1
grep -rl "^status: archived" docs --include="*.md" | grep -v docs/archive && exit 1
```

`docs/README.md` skeleton:

```markdown
# 116 Backend — Documentation

## Start here
- New engineer: [ARCHITECTURE.md](ARCHITECTURE.md) → [testing rules](testing/00-unit-vs-integration-rules.md) → [CLAUDE.md](../CLAUDE.md)
- Operating it: [runbook](operations/runbook.md) — boot, migrate, health, jobs
- Reviewing a stage PR: [audit plan](architecture-audit/implementation-specs/00-implementation-plan.md)

## Rules that are enforced (each links to its test)
| Rule | Enforced by |
| --- | --- |
| Modules touch other modules via Contracts only | `Architecture.Tests/ModuleBoundaryTests` |
| Domain references only Shared.Kernel | `Architecture.Tests/LayerTests` |
| One commit per handler | Stage 13 verification grep |
```

19.6 closes this audit's own loop: each `stage-*.md` gets its front matter plus an
**As built** section recording deviations (the kind Stage 7 accumulated — module subclasses,
`RuleProblem` naming; Stage 8's D8; Stage 9's 19-handler gate), so the specs stop being the
next generation's `[9.10]`.

The cold-start walkthrough is the acceptance test for `[9.4]`: a clean clone, following only
committed docs, reaches a booted app + green suite. Whatever step fails, that doc is the bug.

---

## Verification

1. Link check over `docs/**` — zero dead links.
2. Front-matter CI guard green; `status: current` docs contain no reference to unshipped code.
3. Cold-start walkthrough executed once, by someone other than the author.
4. `git check-ignore CLAUDE.md` → not ignored `[9.1]`.

---

**PR:** `docs: single entry point, truthful guides and one testing rulebook`

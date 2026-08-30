# Spec-Driven Development (SDD) with Claude

## What is SDD?

Spec-Driven Development is the practice of writing a **precise, complete, machine-readable specification** before asking Claude to implement or test anything. The spec is the single source of truth. Claude reads it and produces code that matches it exactly — no guessing, no asking clarifying questions, no invented behavior.

SDD is not documentation. It is a **contract** written in the language of the codebase. It answers every question Claude would otherwise need to ask.

---

## Why SDD works with Claude

Without a spec, Claude:
- Invents field names, method signatures, and error messages
- Makes assumptions about which repositories to call
- Writes tests that don't match the actual implementation
- Produces inconsistent patterns across use cases

With a spec, Claude:
- Generates code that matches the existing architecture exactly
- Writes tests against the correct behavior, not assumed behavior
- Uses the right error classes, the right factory names, the right mock methods
- Produces consistent output every time, even across sessions

---

## The SDD Workflow

```
1. WRITE SPEC
   └── Define intent, command shape, business rules,
       error cases, side effects, response, endpoint, tests

2. VALIDATE SPEC
   └── Read it aloud as if you're the implementer.
       Can you write the code with zero questions? If not, fill the gaps.

3. GIVE SPEC TO CLAUDE
   └── "Implement this spec" — nothing more needed.
       Claude reads the spec and produces all files.

4. REVIEW OUTPUT
   └── Diff against the spec. Every field, every error, every test.

5. ITERATE ON THE SPEC (not the code)
   └── If something is wrong, fix the spec first. Then re-run Claude.
       Never accept "close enough".
```

---

## What a spec covers

Every spec in this codebase covers these sections:

| Section | What it answers |
|---------|----------------|
| **Intent** | What this use case does and why |
| **Command / Query shape** | All fields, types, constraints |
| **Business rules** | Pre-conditions for the operation to succeed |
| **Error cases** | What throws, which exception class, which error factory |
| **Side effects** | What gets persisted, what state changes |
| **Response shape** | Exact return record and fields |
| **Endpoint** | HTTP verb, route, auth policy, rate limiting, Produces |
| **Test cases** | Named list of every test to write |
| **Dependencies** | Repositories, factories, services needed |

If any section is missing, the spec is incomplete. An incomplete spec produces wrong code.

---

## Types of specs

This project uses four spec types, each with its own template:

| Spec type | Template | Use when |
|-----------|----------|----------|
| **Command spec** | `02-command-spec-template.md` | New mutation use case (POST/PUT/PATCH/DELETE) |
| **Query spec** | `03-query-spec-template.md` | New read use case (GET) |
| **Domain entity spec** | `04-domain-entity-spec-template.md` | New domain entity or new domain methods |
| **Factory spec** | `05-factory-spec-template.md` | New application factory (multi-step orchestration) |

---

## File structure Claude will produce

When you give Claude a command spec, it produces:

```
UseCases/Admin/Commands/MyFeature/
├── Contracts/
│   └── IMyFeatureFactory.cs         (if factory involved)
├── V1/
│   └── AdminMyFeatureEndpointV1.cs
├── AdminMyFeatureCommand.cs
├── AdminMyFeatureHandler.cs
├── AdminMyFeatureMetaField.cs
├── AdminMyFeatureValidator.cs        (if inputs need validation)
└── AdminMyFeatureFactory.cs          (if factory involved)

tests/Unit/Modules/Content/Application/Commerce/
└── UseCases/Admin/Commands/MyFeature/
    ├── AdminMyFeatureHandlerTests.cs
    ├── AdminMyFeatureValidatorTests.cs
    └── AdminMyFeatureFactoryTests.cs  (if factory involved)
```

---

## How Claude uses specs in this repo

Claude is already aware of:
- All builders, factories, mocks (documented in `projects/how-to-tests/`)
- All module patterns, naming conventions (documented in `CLAUDE.md`)
- All domain entities, their methods, their error classes

When you write a spec, reference these by name:
- `ContentOrderFactory.CreateSubmitted()` — not "a submitted order"
- `ContentOrderErrors.NotFound(id)` — not "throw a not found error"
- `MockContentOrderRepository.SetupGetByIdWithItems(order)` — not "mock the repository"

The more precise the spec, the less Claude has to infer.

---

## Examples in this folder

| File | What it demonstrates |
|------|---------------------|
| `06-example-command-spec.md` | Full spec for `AdminVerifyPayment` (already implemented) |
| `07-example-query-spec.md` | Full spec for `AdminGetOrderPayment` (already implemented) |
| `08-example-domain-spec.md` | Full spec for `ContentOrderEntity` state machine |
| `09-example-factory-spec.md` | Full spec for `AdminVerifyPaymentFactory` |
| `10-example-new-feature-spec.md` | Full spec for a brand-new feature (walkthrough) |
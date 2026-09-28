#!/usr/bin/env python3
"""Emit the Stage 18 move map: every tracked file under src/ and tests/, old path -> new path.

Deterministic function of `git ls-files`; re-run it after any change to the tree or to the
mapping rules in docs/architecture-audit/implementation-specs/stage-18-project-restructure.md.
Columns: series, kind (move|new|drop), old_path, new_path, refactor_path. `refactor_path` is
where the file lives while the parallel tree exists; `new_path` is where it lives after the swap.
"""

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
OUT = REPO / "docs/architecture-audit/implementation-specs/stage-18-move-map.tsv"

MODULE = {"Content": "Content", "Core": "Storage", "Identity": "Identity", "Mailer": "Mailer"}
LAYERED = {"Content", "Identity", "Storage", "Mailer"}  # every module, owner decision
IDENTITY_CONSTANTS = {"RoleConstants.cs", "SessionConstants.cs", "PermissionConstants.cs", "JwtClaimsConstants.cs", "UserConstants.cs"}
APP_TO_BB_APPLICATION = {"Builders", "Decorators", "DTOs", "Exceptions", "Jobs", "Metadata", "Pagination", "Persistence", "Services"}
APP_TO_BB_PRESENTATION = {"Extensions", "Middleware"}
CLOUDINARY_TO_STORAGE = {"Application/Configurations/CloudinarySettings.cs", "Application/Extensions/CloudinaryExtensions.cs"}

IDENTITY_MOCKS = {
    "MockAuthRepository", "MockOtpRepository", "MockPermissionRepository", "MockRolePermissionRepository",
    "MockRoleRepository", "MockSessionRepository", "MockUserRoleRepository", "MockJwtService", "MockOtpService",
    "MockPasswordService", "MockRefreshTokenService", "MockSessionMetadataService", "MockUserLookupService",
    "MockIdentityUnitOfWork",
}
STORAGE_MOCKS = {
    "MockFileRepository", "MockCloudinaryService", "MockFileService", "MockCoreUnitOfWork", "MockStorageUnitOfWork",
    "MockFileUploadService", "MockImageColorService", "MockCoreRepository", "StubCloudinaryTransport",
}
# Consumed by more than one module's tests, so they live in the module-agnostic tests/TestData library.
AGNOSTIC_MOCKS = {"MockDispatcher", "MockHybridCache", "PassThroughHybridCache"}
# Shared/Application tests whose source moves to Storage under the D4 Cloudinary carve-out.
CLOUDINARY_TESTS_TO_STORAGE = {
    "Shared/Application/Configurations/CloudinarySettingsTests.cs",
    "Shared/Application/Extensions/CloudinaryExtensionsTests.cs",
}


def storagify(segment: str) -> str:
    """Core* / ICore* / MockCore* -> Storage*; migration filenames are historical and never renamed."""
    if re.match(r"^\d{14}_", segment):
        return segment
    return re.sub(r"^(I|Mock|IMock)?Core", lambda m: (m.group(1) or "") + "Storage", segment)


def module_src(m: str, layer: str, sub: str) -> str:
    return f"src/modules/{m}/{m}/src/{m}.{layer}/{sub}"


def module_root(m: str, sub: str) -> str:
    """The module class registers the module's database and services, so it belongs to its outermost layer."""
    return f"src/modules/{m}/{m}/src/{m}.Infrastructure/{sub}"


# Helpers that read one module's vocabulary, measured by their consumers; the rest are genuinely cross-module.
TEST_HELPER_HOMES = {"AuthTestHelpers.cs": "Identity", "ImageTestHelpers.cs": "Content"}


# Tests of shared code that can only run against the whole app: one inventories every module's resource
# families, the other needs an assembly with real handlers. Neither has a module owner (study 05).
WHOLE_APP_UNIT_TESTS = {
    "Shared/Application/Localization/ResourceCompletenessTests.cs",
    "Shared/Application/Extensions/CqrsExtensionTests.cs",
}


def test_data_home(rest: str) -> tuple[str | None, str]:
    """rest = path under tests/Fixtures/ -> (module or None for tests/TestData, sub-path)."""
    seg = rest.split("/")
    base = seg[-1]
    if seg[0] == "Builders":
        if seg[1] == "Commands":
            return "Identity", rest
        if seg[1] in ("Entities", "Requests"):
            return MODULE[seg[2]], f"Builders/{seg[1]}/" + "/".join(seg[3:])
        if seg[1] == "Helpers":
            return "Content", rest
        if base == "AuthDataBuilder.cs":
            return "Identity", "Builders/AuthDataBuilder.cs"
    if seg[0] == "Constants":
        # All 30 files are parts of one partial TestConstants class, imported statically by every integration
        # suite, so they stay one class in one assembly; the module subfolders flatten (namespace = folder).
        return None, "Constants/" + base
    if seg[0] == "Factories":
        if seg[1] == "Shared":
            return None, seg[0] + "/" + "/".join(seg[2:])
        if seg[1] == "Helpers":
            return "Content", rest
        return MODULE[seg[1]], seg[0] + "/" + "/".join(seg[2:])
    if seg[0] == "Helpers" and base in TEST_HELPER_HOMES:
        return TEST_HELPER_HOMES[base], rest
    if seg[0] in ("Helpers", "Routes") or rest in ("TestDataModuleInitializer.cs", ".editorconfig"):
        return None, rest
    raise SystemExit(f"unmapped test-data file: {rest}")


def mock_home(rest: str) -> tuple[str | None, str]:
    """rest = path under tests/Unit/Common/."""
    seg = rest.split("/")
    base = seg[-1]
    stem = base[:-3]
    if seg[0] == "Mocks":
        if stem in AGNOSTIC_MOCKS:
            return None, "Mocks/" + base
        if seg[1] == "Factories":
            return "Content", "Mocks/Factories/" + base
        m = "Identity" if stem in IDENTITY_MOCKS or stem == "MockAvatarService" else "Storage" if stem in STORAGE_MOCKS else "Content"
        return m, f"Mocks/{seg[1]}/" + storagify(base)
    if base == "BaseContentHandlerTest.cs":
        return "Content", base
    return None, rest


# xunit resolves a [CollectionDefinition] only inside the assembly that uses it, so these one-line definitions
# cannot live in a library shared by 7 test assemblies: each suite declares the collections it joins (18.7).
# 18.8 rewrites the container harness for cross-process reuse, so this file is authored rather than moved.
REWRITTEN_FIXTURES = {"tests/Integration/Common/Fixtures/TestPostgresContainer.cs"}

DISSOLVED_COLLECTION_DEFINITIONS = {
    "tests/Unit/Common/EnvironmentVariableCollection.cs",
    *(f"tests/Integration/Common/Fixtures/{n}Collection.cs" for n in
      ("Database", "Seeding", "Resend", "Cors", "RateLimiting", "AccountRateLimiting", "OtpPepperless", "UnreachableDatabase")),
}


def map_file(f: str) -> tuple[str, str | None]:
    """Return (series, new_path); new_path None means the file is retired."""
    p = f.split("/")

    if f.startswith("src/Api/"):
        return "18.6", "src/host/Api/" + f[len("src/Api/"):]

    if f.startswith("src/BuildingBlocks/"):
        rest = f[len("src/BuildingBlocks/"):]
        base = rest.split("/")[-1]
        if base == "BuildingBlocks.csproj":
            return "18.2", None
        if rest.startswith("Constants/") and base in IDENTITY_CONSTANTS:
            return "18.3", f"src/modules/Identity/Identity/src/Identity.Domain/Constants/{base}"
        if base == "FileConstants.cs":
            return "18.3", "src/modules/Storage/Storage/src/Storage.Domain/Constants/FileConstants.cs"
        return "18.2", "src/shared/src/BuildingBlocks.Presentation/" + rest

    if f.startswith("src/Shared/Shared.Contracts/"):
        rest = f[len("src/Shared/Shared.Contracts/"):]
        if rest.endswith(".csproj"):
            return "18.2", None
        return "18.2", "src/shared/src/BuildingBlocks.Application/" + rest.replace("Application/CQRS", "CQRS")

    if f.startswith("src/Shared/Shared/"):
        rest = f[len("src/Shared/Shared/"):]
        if rest.endswith(".csproj"):
            return "18.2", None
        if rest in CLOUDINARY_TO_STORAGE:
            return "18.3", "src/modules/Storage/Storage/src/Storage.Infrastructure/" + rest.split("/")[-1]
        top = rest.split("/")[0]
        if top == "Domain":
            sub = rest[len("Domain/"):]
            if sub == "IRepository.cs":
                return "18.2", "src/shared/src/BuildingBlocks.Domain/IRepository.cs"
            return "18.2", "src/shared/src/Shared.Domain/" + sub
        if top == "Infrastructure":
            sub = rest[len("Infrastructure/"):]
            if sub == "Services/HttpCurrentActor.cs":
                return "18.2", "src/shared/src/BuildingBlocks.Presentation/Services/HttpCurrentActor.cs"
            return "18.2", "src/shared/src/BuildingBlocks.Infrastructure/" + sub
        if top == "Application":
            sub = rest[len("Application/"):]
            group = sub.split("/")[0]
            if group == "Specifications":
                return "18.2", "src/shared/src/BuildingBlocks.Domain/" + sub
            # HTTP rendering of exceptions and rate-limit policy wiring are presentation; a Quartz job contract is infrastructure.
            # IAccountRateLimiter has no dependencies and is consumed by an Application decorator, so only it stays.
            if sub.startswith("Exceptions/Handlers/") or (
                sub.startswith("Builders/RateLimit/") and not sub.endswith("/IAccountRateLimiter.cs")
            ):
                return "18.2", "src/shared/src/BuildingBlocks.Presentation/" + sub
            if group == "Jobs":
                return "18.2", "src/shared/src/BuildingBlocks.Infrastructure/" + sub
            # The env schema is boot configuration read by BaseModule, so it sits below Infrastructure.
            if group == "Configurations":
                return "18.2", "src/shared/src/BuildingBlocks.Application/" + sub
            if group in APP_TO_BB_APPLICATION:
                return "18.2", "src/shared/src/BuildingBlocks.Application/" + sub
            if group in APP_TO_BB_PRESENTATION:
                return "18.2", "src/shared/src/BuildingBlocks.Presentation/" + sub
        raise SystemExit(f"unmapped Shared file: {f}")

    if f.startswith("src/Modules/"):
        old_module = p[2]
        m = MODULE[old_module]
        series = {"Identity": "18.4", "Content": "18.5"}.get(m, "18.6")
        tail = "/".join(p[3:])
        if tail.startswith(f"{old_module}.Contracts/"):
            rest = "/".join(storagify(x) for x in tail[len(f"{old_module}.Contracts/"):].split("/"))
            if rest.endswith(".csproj"):
                return series, f"src/modules/{m}/{m}.Contracts/{m}.Contracts.csproj"
            return series, f"src/modules/{m}/{m}.Contracts/{rest}"
        rest = "/".join(storagify(x) for x in tail[len(old_module) + 1:].split("/"))
        if rest.endswith(".csproj"):
            return series, None if m in LAYERED else f"src/modules/{m}/{m}/src/{m}/{m}.csproj"
        base = rest.split("/")[-1]
        if base == f"{m}Module.cs":
            return series, module_root(m, base)
        layer, sub = rest.split("/", 1)
        return series, module_src(m, layer, sub)

    if f.startswith("tests/Architecture/"):
        rest = f[len("tests/Architecture/"):].replace("_116.Architecture.Tests.csproj", "Architecture.Tests.csproj")
        return "18.7", "tests/Architecture.Tests/" + rest

    if f.startswith("tests/Fixtures/"):
        rest = f[len("tests/Fixtures/"):]
        if rest.endswith(".csproj"):
            return "18.7", None
        m, sub = test_data_home(rest)
        return "18.7", f"src/modules/{m}/{m}/tests/{m}.TestData/{sub}" if m else "tests/TestData/" + sub

    if f.startswith("tests/Unit/"):
        rest = f[len("tests/Unit/"):]
        if rest.endswith(".csproj"):
            return "18.7", None
        if rest.startswith("Modules/"):
            m = MODULE[rest.split("/")[1]]
            sub = "/".join(storagify(x) for x in rest.split("/")[2:])
            return "18.7", f"src/modules/{m}/{m}/tests/{m}.Unit.Tests/{sub}"
        if rest in CLOUDINARY_TESTS_TO_STORAGE:
            return "18.3", "src/modules/Storage/Storage/tests/Storage.Unit.Tests/Infrastructure/" + rest.split("/")[-1]
        if rest in WHOLE_APP_UNIT_TESTS:
            return "18.7", "tests/EndToEnd.Tests/" + rest.split("/")[-1]
        if rest.startswith(("Shared/", "BuildingBlocks/")):
            return "18.7", "src/shared/tests/Shared.Unit.Tests/" + rest
        if rest.startswith("Common/"):
            m, sub = mock_home(rest[len("Common/"):])
            return "18.7", f"src/modules/{m}/{m}/tests/{m}.TestData/{sub}" if m else "tests/TestData/" + sub
        raise SystemExit(f"unmapped unit test: {f}")

    if f.startswith("tests/Integration/"):
        rest = f[len("tests/Integration/"):]
        if rest.endswith(".csproj"):
            return "18.7", None
        if rest.startswith("Modules/"):
            m = MODULE[rest.split("/")[1]]
            sub = "/".join(storagify(x) for x in rest.split("/")[2:])
            return "18.7", f"src/modules/{m}/{m}/tests/{m}.Integration.Tests/{sub}"
        if rest.startswith("Common/"):
            sub = rest[len("Common/"):]
            if sub.startswith("Fixtures/"):
                sub = sub[len("Fixtures/"):]
            return "18.7", "tests/Fixtures/" + sub
        if rest.startswith("Shared/"):
            return "18.7", "src/shared/tests/Shared.Integration.Tests/" + rest[len("Shared/"):]
        return "18.7", "tests/EndToEnd.Tests/" + rest

    if f.startswith("tests/"):
        return "18.11", f

    raise SystemExit(f"unmapped: {f}")


NEW_FILES = [
    ("18.2", "src/shared/src/BuildingBlocks.Presentation/Extensions/CurrentActorExtension.cs"),
    ("18.2", "src/shared/src/Shared.Domain/Constants/LocaleConstants.cs"),
    *[("18.6", f"src/modules/Storage/Storage/src/Storage.Infrastructure/Persistence/Migrations/20260926165924_RenameCoreSchemaToStorage{x}.cs")
      for x in ("", ".Designer")],
    ("18.3", "src/modules/Storage/Storage.Contracts/Domain/Constants/FileUploadLimits.cs"),
    *[("18.2", f"src/shared/src/{n}/{n}.csproj") for n in
      ("Shared.Domain", "BuildingBlocks.Domain", "BuildingBlocks.Application", "BuildingBlocks.Infrastructure", "BuildingBlocks.Presentation")],
    *[(series, f"src/modules/{m}/{m}/src/{m}.{L}/{m}.{L}.csproj")
      for m, series in (("Identity", "18.4"), ("Storage", "18.6"), ("Mailer", "18.6"))
      for L in ("Domain", "Application", "Infrastructure")],
    *[("18.5", f"src/modules/Content/Content/src/Content.{L}/Content.{L}.csproj") for L in ("Domain", "Application", "Infrastructure")],
    *[("18.7", f"src/modules/{m}/{m}/tests/{m}.{t}/{m}.{t}.csproj") for m in ("Content", "Identity", "Storage", "Mailer") for t in ("Unit.Tests", "Integration.Tests")],
    *[("18.7", f"src/modules/{m}/{m}/tests/{m}.TestData/{m}.TestData.csproj") for m in ("Content", "Identity", "Storage", "Mailer")],
    # Mailer had no test data to move: its tests built entities inline, so its builders are written here.
    *[("18.7", f"src/modules/Mailer/Mailer/tests/Mailer.TestData/Builders/Entities/{n}Builder.cs")
      for n in ("Notification", "NewsletterSubscriber", "OutboxEmail")],
    ("18.7", "src/shared/tests/Shared.Unit.Tests/Shared.Unit.Tests.csproj"),
    ("18.7", "src/shared/tests/Shared.Integration.Tests/Shared.Integration.Tests.csproj"),
    ("18.7", "tests/TestData/TestData.csproj"),
    ("18.7", "tests/Fixtures/Fixtures.csproj"),
    ("18.7", "tests/Fixtures/GlobalUsings.cs"),
    ("18.8", "tests/Fixtures/TestPostgresContainer.cs"),
    # D6's behaviour partials, emitted by scripts/stage18-split-entities.py from the state file beside them.
    *[("18.9", f"src/modules/Content/Content/src/Content.Domain/Behaviors/{entity}Entity.{cluster}.cs")
      for entity, clusters in (
          ("Lyrics", ("Creation", "Editorial", "Promotion", "Publication")),
          ("Article", ("Editorial", "Promotion", "Publication")),
          ("Video", ("Editorial", "Promotion", "Publication")),
          ("Category", ("Catalogue", "Pricing")),
          ("Artist", ("Profile", "SocialLinks")),
          ("ContentOrder", ("Composition", "Payment")),
      )
      for cluster in clusters],
    *[("18.9", f"src/modules/Identity/Identity/src/Identity.Domain/Behaviors/UserEntity.{cluster}.cs")
      for cluster in ("Access", "Credentials", "Profile")],
    *[("18.7", f"src/modules/{m}/{m}/tests/{m}.Integration.Tests/AssemblyFixtures.cs") for m in ("Content", "Identity", "Storage", "Mailer")],
    ("18.7", "src/shared/tests/Shared.Integration.Tests/AssemblyFixtures.cs"),
    ("18.7", "tests/EndToEnd.Tests/AssemblyFixtures.cs"),
    *[("18.7", f"src/modules/{m}/{m}/tests/{m}.Integration.Tests/Collections.cs") for m in ("Content", "Identity", "Storage", "Mailer")],
    *[("18.7", f"src/modules/{m}/{m}/tests/{m}.Unit.Tests/Collections.cs") for m in ("Identity", "Mailer")],
    ("18.7", "src/shared/tests/Shared.Integration.Tests/Collections.cs"),
    ("18.7", "src/shared/tests/Shared.Unit.Tests/Collections.cs"),
    ("18.7", "tests/EndToEnd.Tests/Collections.cs"),
    *[("18.7", f"src/modules/{m}/{m}/tests/{m}.Integration.Tests/GlobalUsings.cs") for m in ("Content", "Identity", "Storage", "Mailer")],
    ("18.7", "src/shared/tests/Shared.Integration.Tests/GlobalUsings.cs"),
    ("18.7", "tests/EndToEnd.Tests/EndToEnd.Tests.csproj"),
    *[("18.11", f"src/modules/{m}/{m}.slnf") for m in ("Content", "Identity", "Storage", "Mailer")],
    *[("18.11", f"tests/{category}.slnf") for category in ("unit", "integration")],
    *[("18.11", f"src/modules/{m}/README.md") for m in ("Content", "Identity", "Storage", "Mailer")],
    ("18.11", "116_backend.sln"),
]


def refactor_path(new: str) -> str:
    if new == "116_backend.sln":
        return "116_backend.refactor.sln"
    return re.sub(r"^(src|tests)/", r"\1.refactor/", new)


def main() -> int:
    # The map is computed from the pre-move tree: `src/` + `tests/` before the swap, `src.old/` +
    # `tests.old/` after it. Once those are deleted the generator can no longer reproduce it, and the
    # committed TSV is the record; regenerating from the post-swap tree would emit new -> new rows.
    if not (REPO / "src.old").exists() and (REPO / "src/modules").exists():
        print(
            "refusing to regenerate: the pre-move tree is gone, so "
            "docs/architecture-audit/implementation-specs/stage-18-move-map.tsv is the record",
            file=sys.stderr,
        )
        return 1

    roots = ["src.old", "tests.old"] if (REPO / "src.old").exists() else ["src", "tests"]
    tracked = [
        f.split(".old", 1)[0] + f.split(".old", 1)[1] if ".old" in f else f
        for f in subprocess.run(
            ["git", "-C", str(REPO), "ls-files", *roots], capture_output=True, text=True, check=True
        ).stdout.split()
    ]
    rows: list[tuple[str, str, str, str, str]] = []
    seen: dict[str, str] = {}
    for f in tracked:
        if f in DISSOLVED_COLLECTION_DEFINITIONS:
            rows.append(("18.7", "drop", f, "", ""))
            continue
        if f in REWRITTEN_FIXTURES:
            rows.append(("18.8", "drop", f, "", ""))
            continue
        series, new = map_file(f)
        if new is None:
            rows.append((series, "drop", f, "", ""))
            continue
        if new in seen:
            raise SystemExit(f"collision: {seen[new]} and {f} both map to {new}")
        seen[new] = f
        rows.append((series, "move", f, new, refactor_path(new)))
    for series, new in NEW_FILES:
        if new in seen:
            raise SystemExit(f"new file collides with a move: {new}")
        seen[new] = "(new)"
        rows.append((series, "new", "", new, refactor_path(new)))
    rows.sort(key=lambda r: (r[0], r[1], r[2] or r[3]))
    OUT.write_text("series\tkind\told_path\tnew_path\trefactor_path\n" + "".join("\t".join(r) + "\n" for r in rows))
    moves = sum(1 for r in rows if r[1] == "move")
    drops = sum(1 for r in rows if r[1] == "drop")
    new = sum(1 for r in rows if r[1] == "new")
    projects = sum(1 for r in rows if r[3].endswith(".csproj"))
    print(f"{OUT.relative_to(REPO)}: {moves} moves, {drops} drops, {new} new, {projects} projects")
    return 0


if __name__ == "__main__":
    sys.exit(main())

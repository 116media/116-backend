#!/usr/bin/env python3
"""Copy one Stage 18 series from src/ + tests/ into the parallel tree, rewriting namespaces on the way.

Usage: scripts/stage18-copy-series.py [--clean] [--path=REGEX] 18.2 [18.3 ...]

Reads docs/architecture-audit/implementation-specs/stage-18-move-map.tsv, copies every `move` row
of the given series to its `refactor_path`, and applies the D14 / D7 / D10 namespace rewrites to
.cs files. The old tree is never touched. Re-running is idempotent (files are overwritten);
`--clean` first deletes .cs files inside the series' projects that no map row produces, and `--path=`
keeps only rows whose destination matches, so a series can be copied in dependency order.
"""

from __future__ import annotations

import csv
import os
import re
import shutil
import subprocess
import sys
from collections import defaultdict
from functools import lru_cache
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
MAP = REPO / "docs/architecture-audit/implementation-specs/stage-18-move-map.tsv"

# Each rule rewrites `<old>` when followed by `.`, `;`, `)` or whitespace.
NAMESPACE_RULES: list[tuple[str, str]] = [
    ("_116.Shared.Application.Exceptions.Handlers", "_116.BuildingBlocks.Presentation.Exceptions.Handlers"),
    ("_116.Shared.Application.Builders.RateLimit", "_116.BuildingBlocks.Presentation.Builders.RateLimit"),
    ("_116.Shared.Application.Specifications", "_116.BuildingBlocks.Domain.Specifications"),
    ("_116.Shared.Application.Extensions", "_116.BuildingBlocks.Presentation.Extensions"),
    ("_116.Shared.Application.Middleware", "_116.BuildingBlocks.Presentation.Middleware"),
    ("_116.Shared.Application.Jobs", "_116.BuildingBlocks.Infrastructure.Jobs"),
    ("_116.Shared.Application", "_116.BuildingBlocks.Application"),
    ("_116.Shared.Contracts.Application.CQRS", "_116.BuildingBlocks.Application.CQRS"),
    ("_116.Shared.Infrastructure.Services", "_116.BuildingBlocks.Presentation.Services"),
    ("_116.Shared.Infrastructure", "_116.BuildingBlocks.Infrastructure"),
    ("_116.BuildingBlocks.Constants", "_116.BuildingBlocks.Presentation.Constants"),
    ("_116.BuildingBlocks.Utils", "_116.BuildingBlocks.Presentation.Utils"),
    # The fixtures sat in a Fixtures/ subfolder of Common/; they are the root of the Fixtures project now.
    ("_116.Integration.Tests.Common.Fixtures", "_116.Tests.Fixtures"),
    ("_116.Integration.Tests.Common", "_116.Tests.Fixtures"),
    ("_116.Integration.Tests.Workflows", "_116.EndToEnd.Tests.Workflows"),
    ("_116.Integration.Tests.Api", "_116.EndToEnd.Tests.Api"),
    ("_116.Integration.Tests.Shared", "_116.Shared.Integration.Tests"),
    *[(f"_116.Integration.Tests.Modules.{old}", f"_116.{new}.Integration.Tests")
      for old, new in (("Content", "Content"), ("Identity", "Identity"), ("Core", "Storage"), ("Mailer", "Mailer"))],
    ("_116.Unit.Tests.Common", "_116.Tests.TestData"),
    ("_116.Unit.Tests.Shared", "_116.Shared.Unit.Tests"),
    ("_116.Unit.Tests.BuildingBlocks", "_116.Shared.Unit.Tests"),
    *[(f"_116.Unit.Tests.Modules.{old}", f"_116.{new}.Unit.Tests")
      for old, new in (("Content", "Content"), ("Identity", "Identity"), ("Core", "Storage"), ("Mailer", "Mailer"))],
    ("_116.Tests.Fixtures", "_116.Tests.TestData"),
    ("_116.Core", "_116.Storage"),
]
# One pass, longest prefix first: applied in sequence, a later rule would rewrite an earlier rule's output
# (`_116.Integration.Tests.Common` -> `_116.Tests.Fixtures` -> `_116.Tests.TestData`).
RULE_MAP = dict(NAMESPACE_RULES)
RULE_RX = re.compile(
    "(?:" + "|".join(re.escape(old) for old in sorted(RULE_MAP, key=len, reverse=True)) + r")(?=[.;)\s])"
)

# Files whose own `namespace` declaration differs from what the prefix rules produce.
DECLARED_NAMESPACE_BY_DESTINATION = {
    "src.refactor/shared/src/BuildingBlocks.Domain/IRepository.cs": "_116.BuildingBlocks.Domain",
    "src.refactor/modules/Storage/Storage/src/Storage.Domain/Constants/FileConstants.cs": "_116.Storage.Domain.Constants",
    # D4's Cloudinary carve-out lands in Storage, and a module class now lives in its module's Infrastructure.
    "src.refactor/modules/Storage/Storage/src/Storage.Infrastructure/CloudinarySettings.cs": "_116.Storage.Infrastructure",
    "src.refactor/modules/Storage/Storage/src/Storage.Infrastructure/CloudinaryExtensions.cs": "_116.Storage.Infrastructure",
    "src.refactor/modules/Identity/Identity/src/Identity.Infrastructure/IdentityModule.cs": "_116.Identity.Infrastructure",
    "src.refactor/modules/Content/Content/src/Content.Infrastructure/ContentModule.cs": "_116.Content.Infrastructure",
    "src.refactor/modules/Storage/Storage/src/Storage.Infrastructure/StorageModule.cs": "_116.Storage.Infrastructure",
    "src.refactor/modules/Mailer/Mailer/src/Mailer.Infrastructure/MailerModule.cs": "_116.Mailer.Infrastructure",
    **{
        f"src.refactor/modules/Identity/Identity/src/Identity.Domain/Constants/{n}.cs": "_116.Identity.Domain.Constants"
        for n in ("JwtClaimsConstants", "PermissionConstants", "RoleConstants", "SessionConstants", "UserConstants")
    },
    "src.refactor/shared/src/BuildingBlocks.Application/Builders/RateLimit/IAccountRateLimiter.cs":
        "_116.BuildingBlocks.Application.Builders.RateLimit",
}

TYPE_DECLARATION_RX = re.compile(r"\b(?:class|interface|record|struct|enum)\s+(I?Core\w*)")
CORE_ALIAS_RX = re.compile(r"^using (Core\w+) = _116\.Core", re.M)
STORAGIFY_RX = re.compile(r"^(I|Mock|IMock)?Core")

# D7's four keepers: the migration pair is historical, and the two enums name core *roles* and *content types*,
# which are Identity's and Content's vocabulary, not the module's.
KEEP_CORE_NAMES = {"EnumCoreUserRole", "CoreUserRole", "EnumCoreContentType", "CoreContentType", "InitCoreSchema"}

# D4 carve-outs: members another module reads leave the single-module constants class for a published home.
# type -> member -> (new type, its namespace)
MOVED_MEMBERS: dict[str, dict[str, tuple[str, str]]] = {
    "FileConstants": {
        m: ("FileUploadLimits", "_116.Storage.Contracts.Domain.Constants")
        for m in (
            "MaxVideoFileSizeBytes",
            "AllowedVideoExtensions",
            "MaxAvatarFileSizeBytes",
            "AllowedAvatarMimeTypes",
            "AllowedAvatarExtensions",
        )
    },
    "UserConstants": {
        m: ("LocaleConstants", "_116.Shared.Domain.Constants")
        for m in ("DefaultLocale", "SupportedLocales", "MaxLocaleLength")
    },
}
# The declaration of a moved member, with the doc comment above it, deleted from the class it left.
MEMBER_DECLARATION_RX = "^[ \t]*/// <summary>\n(?:[ \t]*///.*\n)*?[ \t]*public (?:const|static readonly) [\w\[\]?]+ {member} =(?:[^;]*);\n\n?"

# The three shared projects collapse into one reference: a project sees the outermost shared layer it
# needs, since each shared layer references the one below it.
DROPPED_SHARED_PROJECTS = {
    "src/Shared/Shared/Shared.csproj",
    "src/Shared/Shared.Contracts/Shared.Contracts.csproj",
    "src/BuildingBlocks/BuildingBlocks.csproj",
}
SHARED_REPLACEMENT_DEFAULT = "src/shared/src/BuildingBlocks.Presentation/BuildingBlocks.Presentation.csproj"
# Packages a project used to get transitively through the one shared project, now that the graph is split.
EXTRA_PACKAGES_BY_DESTINATION = {
    # Only Program.cs loads the .env file, so the package belongs to the host rather than the shared graph.
    "src/host/Api/Api.csproj": ("DotNetEnv",),
}

# A layered module's own project is dropped; whatever referenced it now references its outermost layer.
DROPPED_MODULE_PROJECTS = {
    f"src/Modules/{old}/{old}/{old}.csproj": f"src/modules/{new}/{new}/src/{new}.Infrastructure/{new}.Infrastructure.csproj"
    for old, new in (("Identity", "Identity"), ("Content", "Content"), ("Core", "Storage"), ("Mailer", "Mailer"))
}
SHARED_REPLACEMENT_BY_DESTINATION = {
    # A published seam carries DTOs and enums, so it stops at the application layer.
    "src/modules/Storage/Storage.Contracts/Storage.Contracts.csproj": "src/shared/src/BuildingBlocks.Application/BuildingBlocks.Application.csproj",
}

PROJECT_REFERENCE_RX = re.compile(r'^([ \t]*)<ProjectReference Include="([^"]+)" />\n', re.M)
INTERNALS_VISIBLE_TO_RX = re.compile(r'<InternalsVisibleTo Include="([^"]+)" />')
ROOT_NAMESPACE_RX = re.compile(r"<RootNamespace>([\w.]+)</RootNamespace>")

# Shared-kernel layers by namespace prefix, innermost first; a `using` may only point at the same or an inner layer.
SHARED_LAYERS = [
    "_116.Shared.Domain",
    "_116.BuildingBlocks.Domain",
    "_116.BuildingBlocks.Application",
    "_116.BuildingBlocks.Infrastructure",
    "_116.BuildingBlocks.Presentation",
]
SHARED_PROJECT_DIRS = ["Shared.Domain", "BuildingBlocks.Domain", "BuildingBlocks.Application", "BuildingBlocks.Infrastructure", "BuildingBlocks.Presentation"]

MODULE_DOMAIN_RX = re.compile(r"/modules/([^/]+)/\1/src/\1\.Domain/")
TEST_DESTINATION_RX = re.compile(r"^tests\.refactor/|/tests/")
NAMESPACE_RX = re.compile(r"^namespace ([\w.]+);", re.M)
TYPE_DECL_RX = re.compile(
    r"^(?:public|internal)\s+(?:(?:static|sealed|abstract|partial|readonly|unsafe)\s+)*(?:class|record(?:\s+(?:struct|class))?|interface|struct|enum)\s+(\w+)",
    re.M,
)
USING_RX = re.compile(r"^using (_116[\w.]+);\n", re.M)
USING_ALIAS_RX = re.compile(r"^using (\w+) = (_116[\w.]+)\.(\w+);$", re.M)
COMMENT_RX = re.compile(r"^[ \t]*//.*$", re.M)


def rewrite_namespace(ns: str) -> str:
    return rewrite_prefixes(ns + ";")[:-1]


def rewrite_prefixes(text: str) -> str:
    return RULE_RX.sub(lambda m: RULE_MAP[m.group(0)], text)


def declared_namespace(text: str) -> str | None:
    m = NAMESPACE_RX.search(text)
    return m.group(1) if m else None


STATIC_METHOD_RX = re.compile(r"public static [\w<>,\[\]?. ]+? (\w+)(?:<[^>()]*>)?\(")


def declared_namespace_for(row: dict) -> str | None:
    old_ns = declared_namespace((REPO / row["old_path"]).read_text(encoding="utf-8"))
    if old_ns is None:
        return None
    return (
        DECLARED_NAMESPACE_BY_DESTINATION.get(row["refactor_path"])
        or folder_namespace(row["refactor_path"])
        or rewrite_namespace(old_ns)
    )


@lru_cache(maxsize=None)
def type_index() -> tuple[dict[str, set[str]], dict[str, set[str]], dict[str, set[str]]]:
    """Top-level type names by *rewritten* namespace across the old tree, plus the types whose namespace moved
    away from what the prefix rules alone would give (they need an explicit `using` wherever they are referenced),
    plus those types' static method names — an extension method is called without ever naming its class. A name
    declared in several new namespaces maps to all of them; the consumer's own reference graph picks one."""
    by_namespace: dict[str, set[str]] = defaultdict(set)
    moved: dict[str, set[str]] = {}
    moved_methods: dict[str, set[str]] = defaultdict(set)
    for r in csv.DictReader(MAP.open(), delimiter="\t"):
        if r["kind"] != "move" or not r["old_path"].endswith(".cs") or not r["refactor_path"]:
            continue
        text = (REPO / r["old_path"]).read_text(encoding="utf-8")
        old_ns = declared_namespace(text)
        if old_ns is None:
            continue
        new_ns = (
            DECLARED_NAMESPACE_BY_DESTINATION.get(r["refactor_path"])
            or folder_namespace(r["refactor_path"])
            or rewrite_namespace(old_ns)
        )
        names = {core_renames().get(n, n) for n in TYPE_DECL_RX.findall(text)}
        by_namespace[new_ns] |= names
        if new_ns != rewrite_namespace(old_ns):
            for n in names:
                moved.setdefault(n, set()).add(new_ns)
            moved_methods[new_ns] |= set(STATIC_METHOD_RX.findall(text))
    return by_namespace, moved, moved_methods


@lru_cache(maxsize=None)
def project_of(refactor_path: str) -> str | None:
    """The project file a destination belongs to: its nearest ancestor holding one, or None for a dropped file."""
    if not refactor_path:
        return None
    d = (REPO / refactor_path).parent
    while d != REPO and REPO in d.parents and not any(d.glob("*.csproj")):
        d = d.parent
    proj = next(iter(d.glob("*.csproj")), None)
    return proj.relative_to(REPO).as_posix() if proj else None


@lru_cache(maxsize=None)
def reachable_projects(project: str) -> frozenset[str]:
    """A project plus every project it references, transitively — the compiler's own visibility rule."""
    seen: set[str] = set()
    stack = [project]
    while stack:
        p = stack.pop()
        if p in seen or not (REPO / p).exists():
            continue
        seen.add(p)
        base = (REPO / p).parent
        for m in PROJECT_REFERENCE_RX.finditer((REPO / p).read_text(encoding="utf-8")):
            target = os.path.normpath(os.path.join(base, m.group(2).replace("\\", "/")))
            stack.append(Path(target).relative_to(REPO).as_posix())
    return frozenset(seen)


@lru_cache(maxsize=None)
def namespace_owners() -> dict[str, frozenset[str]]:
    """Every `_116.*` namespace the new tree declares (with ancestors) -> the projects that declare it."""
    owners: dict[str, set[str]] = defaultdict(set)
    for r in csv.DictReader(MAP.open(), delimiter="\t"):
        if not r["refactor_path"].endswith(".cs"):
            continue
        if r["old_path"]:
            ns = declared_namespace_for(r)
        else:
            # Scaffolding written by hand (constants carved out by D4, collection definitions, builders):
            # it is already at its destination, and files that reference it need it to be visible.
            destination = REPO / r["refactor_path"]
            ns = declared_namespace(destination.read_text(encoding="utf-8")) if destination.exists() else None
        project = project_of(r["refactor_path"])
        if ns is None or project is None:
            continue
        parts = ns.split(".")
        for i in range(len(parts)):
            owners[".".join(parts[: i + 1])].add(project)
    return {k: frozenset(v) for k, v in owners.items()}


def visible(ns: str, refactor_path: str) -> bool:
    """Whether a `using` of this namespace resolves from the project the file lands in."""
    owners = namespace_owners().get(ns)
    if owners is None:
        return False
    project = project_of(refactor_path)
    return project is None or bool(owners & reachable_projects(project))


@lru_cache(maxsize=None)
def known_namespaces() -> frozenset[str]:
    """Every `_116.*` namespace the new tree declares, with their ancestors — a `using` of anything else is a
    namespace the move dissolved, and would not compile."""
    names: set[str] = set()
    for ns in type_index()[0]:
        parts = ns.split(".")
        names |= {".".join(parts[: i + 1]) for i in range(len(parts))}
    return frozenset(names)


def is_self_or_ancestor(ns: str, own: str | None) -> bool:
    return own is not None and (own == ns or own.startswith(ns + "."))


IDENTIFIER_RX = re.compile(r"\b\w+\b")
MEMBER_CALL_RX = re.compile(r"\.(\w+)\s*[(<]")


def code_only(text: str) -> str:
    """A doc comment naming a type is not a reference to it, and must not pull in a `using`."""
    return COMMENT_RX.sub("", text)


def would_be_ambiguous(name: str, imported: list[str]) -> bool:
    """Whether some namespace the file already imports declares this name too, so importing another would
    turn every use of it into CS0104 — which happens wherever a test double is named after the type it doubles."""
    by_namespace = type_index()[0]
    return any(name in by_namespace.get(ns, ()) for ns in imported)


def shared_layer(ns_or_path: str) -> int | None:
    for i, prefix in enumerate(SHARED_LAYERS):
        if ns_or_path == prefix or ns_or_path.startswith(prefix + "."):
            return i
    return None


def fix_usings(text: str, old_ns: str | None, refactor_path: str) -> str:
    """Namespaces moved between projects, so implicit parent-namespace scope and unchanged `using` lines no longer
    line up with where the types are: add the usings a file needs, drop the ones its layer cannot reach."""
    by_namespace, moved, moved_methods = type_index()
    own = declared_namespace(text)
    body = code_only(text)
    # One tokenisation, then set intersections: scanning per candidate name is thousands of passes per file.
    referenced = set(IDENTIFIER_RX.findall(body))
    called = set(MEMBER_CALL_RX.findall(body))
    imported = USING_RX.findall(text)
    needed: list[str] = []
    chain = []
    if old_ns:
        parts = old_ns.split(".")
        chain = [".".join(parts[:i]) for i in range(len(parts), 1, -1)]
    for ancestor in chain:
        ns = rewrite_namespace(ancestor)
        if is_self_or_ancestor(ns, own) or ns == old_ns and ns == own:
            continue
        if by_namespace.get(ns, frozenset()) & referenced:
            needed.append(ns)
    for name in referenced:
        candidates = moved.get(name)
        if not candidates or would_be_ambiguous(name, imported):
            continue
        reachable = [ns for ns in candidates if visible(ns, refactor_path) and not is_self_or_ancestor(ns, own)]
        if len(reachable) == 1:
            needed.append(reachable[0])
    for ns, methods in moved_methods.items():
        if not is_self_or_ancestor(ns, own) and methods & called and visible(ns, refactor_path):
            needed.append(ns)
    for members in MOVED_MEMBERS.values():
        for member, (new_type, ns) in members.items():
            if not is_self_or_ancestor(ns, own) and f"{new_type}.{member}" in body:
                needed.append(ns)
    def realias(m: re.Match[str]) -> str:
        alias, ns, name = m.groups()
        if visible(ns, refactor_path):
            return m.group(0)
        reachable = [c for c in moved.get(name, ()) if visible(c, refactor_path)]
        return f"using {alias} = {reachable[0]}.{name};" if len(reachable) == 1 else m.group(0)

    text = USING_ALIAS_RX.sub(realias, text)
    text = USING_RX.sub(lambda m: "" if not visible(m.group(1), refactor_path) else m.group(0), text)
    file_layer = next((i for i, d in enumerate(SHARED_PROJECT_DIRS) if f"/{d}/" in refactor_path), None)
    if MODULE_DOMAIN_RX.search(refactor_path):
        # A module's Domain project reaches only the two domain layers of the shared kernel, so a `using` of
        # an outer one is a constant that went home to this module (D4) and is named through the type index.
        file_layer = SHARED_PROJECT_DIRS.index("BuildingBlocks.Domain")
    if file_layer is not None:
        text = USING_RX.sub(lambda m: "" if (shared_layer(m.group(1)) or 0) > file_layer else m.group(0), text)
    for ns in dict.fromkeys(needed):
        if f"using {ns};\n" not in text:
            text = f"using {ns};\n" + text
    return text


MAILER_ENTITY_ARGUMENTS = {
    "NotificationEntity.Enqueue": None,
    "NotificationEntity.Create": ("id", "userId", "type", "title", "body", "linkPath"),
    "OutboxEmailEntity.Enqueue": ("id", "recipientAddress", "recipientName", "subject", "htmlBody", "textBody", "template", "now"),
}
MAILER_BUILDER_CHAIN = {
    "NotificationEntity.Create": (
        "NotificationBuilder",
        (("WithId", ("id",)), ("WithUserId", ("userId",)), ("WithType", ("type",)),
         ("WithContent", ("title", "body")), ("WithLinkPath", ("linkPath",))),
    ),
    "OutboxEmailEntity.Enqueue": (
        "OutboxEmailBuilder",
        (("WithId", ("id",)), ("WithRecipient", ("recipientAddress", "recipientName")),
         ("WithContent", ("subject", "htmlBody", "textBody")), ("WithTemplate", ("template",)), ("At", ("now",))),
    ),
}
SUBSCRIBE_RX = re.compile(
    r"(?P<indent>[ \t]*)(?P<decl>(?:var|NewsletterSubscriberEntity) (?P<name>\w+) = |return )"
    r"NewsletterSubscriberEntity\.Subscribe\((?P<id>[^,]+), (?P<email>[^)]+)\);\n"
    r"(?P<follow>(?:[ \t]*\w+\.(?:Confirm|Unsubscribe)\([^)]*\);\n)*)"
)
FACTORY_CALL_RX = re.compile(
    r"(?P<indent>[ \t]*)(?P<lead>(?:var|\w+) \w+ = |return )(?P<call>NotificationEntity\.Create|OutboxEmailEntity\.Enqueue)\(\n"
    r"(?P<args>(?:[^;]*?))\n[ \t]*\);\n"
)


def mailer_test_data_builders(text: str) -> str:
    """Mailer had no test-data library, so its tests built entities inline; they go through builders in
    `Mailer.TestData` like the other three modules. Entity guard tests keep calling the factory directly,
    because they exist to pass it values a builder would never produce."""

    def subscriber(m: re.Match[str]) -> str:
        follow = m.group("follow")
        state = ".AsUnsubscribed()" if ".Unsubscribe(" in follow else ".AsConfirmed()" if ".Confirm(" in follow else ""
        return (
            f"{m.group('indent')}{m.group('decl')}new NewsletterSubscriberBuilder()"
            f".WithId({m.group('id').strip()}).WithEmail({m.group('email').strip()}){state}.Build();\n"
        )

    def factory(m: re.Match[str]) -> str:
        builder, chain = MAILER_BUILDER_CHAIN[m.group("call")]
        names = MAILER_ENTITY_ARGUMENTS[m.group("call")]
        values: dict[str, str] = {}
        for i, raw in enumerate(a.strip().rstrip(",").strip() for a in m.group("args").split(",\n")):
            if ": " in raw and raw.split(": ", 1)[0].strip().isidentifier():
                key, value = raw.split(": ", 1)
                values[key.strip()] = value.strip()
            else:
                values[names[i]] = raw
        calls = "".join(
            f".{method}({', '.join(values[p] for p in params)})"
            for method, params in chain
            if all(p in values for p in params)
        )
        return f"{m.group('indent')}{m.group('lead')}new {builder}(){calls}.Build();\n"

    new = SUBSCRIBE_RX.sub(subscriber, text)
    new = FACTORY_CALL_RX.sub(factory, new)
    if new != text:
        using = "using _116.Mailer.TestData.Builders.Entities;\n"
        if using not in new:
            new = using + new
    return new


# Edits a specific destination needs beyond namespace rewriting, applied on every copy so the series is reproducible.
def post_process(text: str, refactor_path: str) -> str:
    if refactor_path.endswith("/BuildingBlocks.Domain/IRepository.cs"):
        # The interface documented an exception only its Infrastructure implementation can throw.
        text = text.replace("using _116.BuildingBlocks.Application.Exceptions;\n", "").lstrip("\n")
        text = re.sub(r"^[ \t]*/// <exception cref=\"NotFoundException\">.*\n", "", text, flags=re.M)
    if refactor_path.endswith("/Shared.Domain/IAggregateRoot.cs"):
        # IRepository now lives one layer out (D3), so the cref can no longer resolve from Shared.Domain.
        text = text.replace('<see cref="IRepository{TEntity,TId}" />', "<c>IRepository&lt;TEntity, TId&gt;</c>")
    own_type = Path(refactor_path).stem
    for member, (new_type, _) in MOVED_MEMBERS.get(own_type, {}).items():
        text = re.sub(MEMBER_DECLARATION_RX.format(member=member), "", text, count=1, flags=re.M)
        assert f"{member} =" not in text, f"{own_type}.{member} not removed from {refactor_path}"
    for old_type, members in MOVED_MEMBERS.items():
        if old_type == own_type:
            continue
        new_type = next(iter(members.values()))[0]
        for member, _ in members.items():
            text = text.replace(f"{old_type}.{member}", f"{new_type}.{member}")
        if re.search(rf"\b{old_type}\b", text) and not any(f"{old_type}.{m}" in text for m in members_left_behind(old_type)):
            text = re.sub(rf"\b{old_type}\b", new_type, text)
    if "services.AddModuleDatabase(" in text:
        # BaseModule can no longer register the HTTP-backed ICurrentActor the audit interceptor needs, because
        # HttpCurrentActor is presentation now (D2). Whoever registers a module database registers the actor, so
        # a module still stands up in a bare container.
        text = re.sub(
            r"^([ \t]*)services\.AddModuleDatabase\(",
            lambda m: f"{m.group(1)}services.AddHttpCurrentActor();\n{m.group(1)}services.AddModuleDatabase(",
            text,
            flags=re.M,
        )
        using = "using _116.BuildingBlocks.Presentation.Extensions;\n"
        if using not in text:
            text = using + text
    if refactor_path.endswith(".runsettings"):
        # The test libraries are test code: `*Tests*` caught the old `_116.Tests.Fixtures` assembly, but not
        # `Fixtures`, `TestData` or `<M>.TestData`. IncludeDirectory goes because the suites no longer sit at one
        # depth — a path relative to the test project cannot name `src/` for both `tests/` and `src/modules/…/tests/`.
        text = text.replace(
            "<Exclude>[*Tests*]*,[*Migrations*]*</Exclude>",
            "<Exclude>[*Tests*]*,[*Migrations*]*,[Fixtures]*,[TestData]*,[*.TestData]*</Exclude>",
        )
        text = re.sub(r"^[ \t]*<IncludeDirectory>.*\n", "", text, flags=re.M)
    if "/Architecture.Tests/" in refactor_path:
        # A module is three assemblies now, and the rules filter types by namespace, so they must scan all
        # three rather than the one the module class happens to live in.
        text = text.replace("new(\"Core\", typeof(StorageModule).Assembly)", "new(\"Storage\", typeof(StorageModule).Assembly)")
        text = text.replace("Types.InAssembly(module.Assembly)", "Types.InAssemblies(module.Assemblies)")
        text = text.replace(".InAssembly(module.Assembly)", ".InAssemblies(module.Assemblies)")
        text = text.replace(
            """    /// <summary>
    /// The module's namespace root, for example <c>_116.Content</c>.
    /// </summary>
    public string Root => $"_116.{Name}";""",
            """    /// <summary>
    /// The module's namespace root, for example <c>_116.Content</c>.
    /// </summary>
    public string Root => $"_116.{Name}";

    /// <summary>
    /// The module's three layer assemblies. Loaded by name because the module class only identifies the
    /// outermost one, while the rules below filter types across every layer.
    /// </summary>
    public IEnumerable<Assembly> Assemblies =>
        new[] { "Domain", "Application", "Infrastructure" }.Select(layer => Assembly.Load($"{Name}.{layer}"));""",
        )
    if refactor_path.endswith("/Fixtures/PostgresFixture.cs"):
        # One container now serves every assembly (D9), so a name derived from the fixture type alone
        # would collide between two modules' identically named fixtures.
        old_name = 'protected virtual string DatabaseName => $"test_116_{GetType().Name.ToLowerInvariant()}";'
        new_name = (
            '    /// <summary>\n'
            '    /// The test assembly this fixture runs in, as a database-name fragment: its first segment, which\n'
            '    /// names the suite while leaving room under the 63-byte identifier limit, past which PostgreSQL\n'
            '    /// truncates silently, so two fixtures whose names then matched would share one database.\n'
            '    /// Each xunit v3 suite is its own executable, so the entry assembly is the suite, not this library.\n'
            '    /// </summary>\n'
            '    private static string AssemblyPrefix =>\n'
            '        (Assembly.GetEntryAssembly()?.GetName().Name ?? "tests").Split(separator: \'.\')[0].ToLowerInvariant();\n'
            '\n'
            '    /// <summary>\n'
            '    /// The database this fixture leases, named for the assembly and the fixture type so that no two\n'
            '    /// fixtures collide on the container every assembly shares.\n'
            '    /// </summary>\n'
            '    protected virtual string DatabaseName => $"test_116_{AssemblyPrefix}_{GetType().Name.ToLowerInvariant()}";'
        )
        assert old_name in text
        text = text.replace("    " + old_name, new_name)
        text = re.sub(
            r"^    /// <summary>\n    /// The database this fixture leases\. Derived from the fixture type so that every fixture\n"
            r"    /// gets its own database on the shared container without a name having to be maintained\.\n    /// </summary>\n",
            "",
            text,
            flags=re.M,
        )
        if "using System.Reflection;\n" not in text:
            text = "using System.Reflection;\n" + text
    if "/modules/Mailer/Mailer/tests/" in refactor_path and not refactor_path.endswith("EntityTests.cs"):
        text = mailer_test_data_builders(text)
    if refactor_path.endswith("/host/Api/Program.cs"):
        # Handlers, endpoints and validators live in <M>.Application; the module class is in <M>.Infrastructure
        # now, so scanning its assembly would find nothing to register or decorate.
        for old, new in (
            ("typeof(StorageModule).Assembly", "typeof(StorageI18n).Assembly"),
            ("typeof(IdentityModule).Assembly", "typeof(IdentityI18n).Assembly"),
            ("typeof(ContentModule).Assembly", "typeof(ContentI18n).Assembly"),
            ("typeof(MailerModule).Assembly", "typeof(MailerValidation).Assembly"),
        ):
            assert old in text, old
            text = text.replace(old, new)
        for using in (
            "using _116.Storage.Application.Shared.Errors.Facade;\n",
            "using _116.Identity.Application.Shared.Errors.Facade;\n",
            "using _116.Content.Application.Shared.Errors.Facade;\n",
            "using _116.Mailer.Application.Shared.Validators;\n",
        ):
            if using not in text:
                text = using + text
    if refactor_path.endswith("/EndToEnd.Tests/ResourceCompletenessTests.cs"):
        # The localized message catalogues are in each module's Application layer; the module class only
        # identifies its Infrastructure assembly, so scanning that finds no resources at all.
        for old, new in (
            ("typeof(StorageModule).Assembly", "typeof(StorageI18n).Assembly"),
            ("typeof(IdentityModule).Assembly", "typeof(IdentityI18n).Assembly"),
            ("typeof(ContentModule).Assembly", "typeof(ContentI18n).Assembly"),
            ("typeof(MailerModule).Assembly", "typeof(MailerValidation).Assembly"),
        ):
            assert old in text, old
            text = text.replace(old, new)
        for using in (
            "using _116.Storage.Application.Shared.Errors.Facade;\n",
            "using _116.Identity.Application.Shared.Errors.Facade;\n",
            "using _116.Content.Application.Shared.Errors.Facade;\n",
            "using _116.Mailer.Application.Shared.Validators;\n",
        ):
            if using not in text:
                text = using + text
    if refactor_path.endswith("/EndToEnd.Tests/CqrsExtensionTests.cs"):
        # The module assemblies are per layer now; the handlers this scans for are in the application layer.
        text = text.replace('Assembly.Load("Identity")', 'Assembly.Load("Identity.Application")')
    if refactor_path.endswith("/BuildingBlocks.Infrastructure/BaseModule.cs"):
        # The HTTP-backed ICurrentActor registration moved to Presentation (CurrentActorExtension.AddHttpCurrentActor).
        for line in (
            "using _116.BuildingBlocks.Presentation.Services;\n",
            "        services.AddHttpContextAccessor();\n",
            "        services.TryAddSingleton<ICurrentActor, HttpCurrentActor>();\n\n",
        ):
            assert line in text, line
            text = text.replace(line, "")
    return text


def rewrite(text: str, refactor_path: str) -> str:
    old_ns = declared_namespace(text)
    text = rewrite_prefixes(text)
    text = rename_core_identifiers(text)
    text = rename_core_text(text, refactor_path)
    override = DECLARED_NAMESPACE_BY_DESTINATION.get(refactor_path) or folder_namespace(refactor_path)
    if override:
        text = NAMESPACE_RX.sub(f"namespace {override};", text, count=1)
    text = post_process(text, refactor_path)
    return fix_usings(text, old_ns, refactor_path)


@lru_cache(maxsize=None)
def project_root_namespace(project: str) -> str | None:
    """The `RootNamespace` declared by a project file."""
    m = ROOT_NAMESPACE_RX.search((REPO / project).read_text(encoding="utf-8"))
    return m.group(1) if m else None


def folder_namespace(refactor_path: str) -> str | None:
    """Namespace derived from the destination: the project's root namespace plus the folders below it. The test
    tree is re-cut into 16 projects, so a test's namespace follows where the file lands rather than where it was."""
    if not refactor_path or not TEST_DESTINATION_RX.search(refactor_path):
        return None
    project = project_of(refactor_path)
    root = project_root_namespace(project) if project else None
    if root is None:
        return None
    folders = Path(refactor_path).parent.relative_to(Path(project).parent).parts
    return ".".join([root, *folders])


@lru_cache(maxsize=None)
def members_left_behind(type_name: str) -> tuple[str, ...]:
    """The members a split constants class keeps, so a consumer that names none of them is naming the
    published half — a doc `cref` to the class itself, which has to follow the members it describes."""
    row = next(
        r for r in csv.DictReader(MAP.open(), delimiter="\t") if r["old_path"].endswith(f"/{type_name}.cs")
    )
    text = (REPO / row["old_path"]).read_text(encoding="utf-8")
    declared = re.findall(r"public (?:const|static readonly) [\w\[\]?]+ (\w+) =", text)
    return tuple(m for m in declared if m not in MOVED_MEMBERS[type_name])


@lru_cache(maxsize=None)
def core_renames() -> dict[str, str]:
    """`Core*` names that are the module's own: the types it declares, its test classes and mocks, and the
    aliases pointing into its namespace. Computed, not matched by prefix — `CoreEventId` is EF Core's own
    type and Identity's `CoreRoleCannotBeDeleted` / `CoreUserRole` name core *roles*, so a prefix rule would
    rename all three."""
    names: set[str] = set()
    for r in csv.DictReader(MAP.open(), delimiter="\t"):
        if not r["old_path"].endswith(".cs"):
            continue
        text = (REPO / r["old_path"]).read_text(encoding="utf-8")
        if "/Core/" in r["old_path"]:
            names |= set(TYPE_DECLARATION_RX.findall(text))
        names |= set(CORE_ALIAS_RX.findall(text))
    names -= KEEP_CORE_NAMES
    return {n: STORAGIFY_RX.sub(lambda m: (m.group(1) or "") + "Storage", n) for n in names}


@lru_cache(maxsize=None)
def core_rename_rx() -> re.Pattern[str]:
    return re.compile(r"\b(" + "|".join(sorted(core_renames(), key=len, reverse=True)) + r")\b")


def rename_core_identifiers(text: str) -> str:
    return core_rename_rx().sub(lambda m: core_renames()[m.group(1)], text)


# The module's prose, its schema and its rule codes carry the old name too; migrations are history and are
# never rewritten — the schema rename is a new migration (D7), not an edit to the old ones.
def rename_core_text(text: str, refactor_path: str) -> str:
    # The model snapshot describes the model as it is now, not as it was, so the schema rename reaches it;
    # everything else under Migrations/ is history and is left alone.
    if "/Migrations/" in refactor_path and not refactor_path.endswith("ModelSnapshot.cs"):
        return text
    text = text.replace("core.file.", "storage.file.").replace("core.files", "storage.files")
    if "/modules/Storage/" in refactor_path:
        text = text.replace('"core"', '"storage"')
        # Not the framework's own name: "Entity Framework Core" and "EF Core" are not this module.
        text = re.sub(r"(?<!Entity Framework )(?<!EF )(?<!ASP\.NET )(?<!\.NET )\bCore\b", "Storage", text)
    return text


@lru_cache(maxsize=None)
def project_index() -> tuple[dict[str, str], dict[str, str]]:
    """Every project file by old path and by new path, each mapped to its path in the parallel tree."""
    by_old: dict[str, str] = {}
    by_new: dict[str, str] = {}
    for r in csv.DictReader(MAP.open(), delimiter="\t"):
        if not r["new_path"].endswith(".csproj"):
            continue
        by_new[r["new_path"]] = r["refactor_path"]
        if r["old_path"]:
            by_old[r["old_path"]] = r["refactor_path"]
    return by_old, by_new


def module_of(refactor_path: str) -> str | None:
    m = re.search(r"/modules/([^/]+)/", refactor_path)
    return m.group(1) if m else None


def rewrite_csproj(text: str, old_path: str, refactor_path: str, new_path: str) -> str:
    """Re-point every ProjectReference at the parallel tree, collapsing the three dropped shared projects
    into the one shared layer this project needs, and rename the root namespace and friend assemblies."""
    by_old, by_new = project_index()
    replacement = SHARED_REPLACEMENT_BY_DESTINATION.get(new_path, SHARED_REPLACEMENT_DEFAULT)
    old_dir, new_dir = os.path.dirname(old_path), os.path.dirname(refactor_path)
    seen: set[str] = set()

    def one(m: re.Match[str]) -> str:
        # Some project files spell their references with Windows separators.
        target = os.path.normpath(os.path.join(old_dir, m.group(2).replace("\\", "/"))).replace(os.sep, "/")
        if target in DROPPED_SHARED_PROJECTS:
            new_target = by_new[replacement]
        elif target in DROPPED_MODULE_PROJECTS:
            new_target = by_new[DROPPED_MODULE_PROJECTS[target]]
        else:
            new_target = by_old[target]
        rel = os.path.relpath(new_target, new_dir).replace(os.sep, "/")
        if rel in seen:
            return ""
        seen.add(rel)
        return f'{m.group(1)}<ProjectReference Include="{rel}" />\n'

    text = PROJECT_REFERENCE_RX.sub(one, text)
    text = ROOT_NAMESPACE_RX.sub(lambda m: f"<RootNamespace>{rewrite_prefixes(m.group(1) + ';')[:-1]}</RootNamespace>", text)
    module = module_of(refactor_path)
    friends = {
        "_116.Unit.Tests": f"{module}.Unit.Tests",
        "_116.Integration.Tests": f"{module}.Integration.Tests",
        "_116.Tests.Fixtures": f"{module}.TestData",
        # The assembly that constructs `StoredFile` through its internal constructor is the outermost layer now.
        "Core": "Storage.Infrastructure",
    }
    text = INTERNALS_VISIBLE_TO_RX.sub(
        lambda m: f'<InternalsVisibleTo Include="{friends.get(m.group(1), m.group(1))}" />', text
    )
    for package in EXTRA_PACKAGES_BY_DESTINATION.get(new_path, ()):
        first = re.search(r'^[ \t]*<PackageReference Include="[^"]+" />\n', text, re.M)
        assert first, new_path
        text = text[: first.start()] + f'        <PackageReference Include="{package}" />\n' + text[first.start() :]
    return text


def clean(rows: list[dict], series: list[str]) -> int:
    """Delete .cs files inside the series' projects that no map row produces, so re-runs after a placement change leave no strays."""
    keep = {r["refactor_path"] for r in csv.DictReader(MAP.open(), delimiter="\t") if r["refactor_path"]}
    project_dirs = [Path(r["refactor_path"]).parent for r in rows if r["series"] in series and r["refactor_path"].endswith(".csproj")]
    removed = 0
    for d in project_dirs:
        for f in (REPO / d).rglob("*.cs"):
            rel = f.relative_to(REPO).as_posix()
            if "/obj/" in rel or "/bin/" in rel or rel in keep:
                continue
            f.unlink()
            removed += 1
    return removed


def main(argv: list[str]) -> int:
    do_clean = "--clean" in argv
    path_filter = next((a[len("--path="):] for a in argv if a.startswith("--path=")), None)
    series = [a for a in argv if not a.startswith("--")]
    all_rows = list(csv.DictReader(MAP.open(), delimiter="\t"))
    if path_filter:
        keep = re.compile(path_filter)
        all_rows = [r for r in all_rows if keep.search(r["refactor_path"])]
    if do_clean:
        print(f"clean: removed {clean(all_rows, series)} stray files")
    rows = [r for r in all_rows if r["series"] in series and r["kind"] == "move"]
    if not rows:
        raise SystemExit(f"no move rows for series {series}")
    copied = rewritten = 0
    for r in rows:
        src = REPO / r["old_path"]
        dst = REPO / r["refactor_path"]
        dst.parent.mkdir(parents=True, exist_ok=True)
        if src.suffix in (".cs", ".csproj", ".runsettings"):
            text = src.read_text(encoding="utf-8")
            if src.suffix == ".csproj":
                new = rewrite_csproj(text, r["old_path"], r["refactor_path"], r["new_path"])
            else:
                new = rewrite(text, r["refactor_path"])
            dst.write_text(new, encoding="utf-8")
            rewritten += new != text
        else:
            shutil.copy2(src, dst)
        copied += 1
    print(f"series {', '.join(series)}: copied {copied} files, {rewritten} with namespace rewrites")
    # The oversized entity files are split into behaviour partials (D6) after the copy restores them whole.
    if any(r["refactor_path"].endswith("Entity.cs") and ".Domain/Entities/" in r["refactor_path"] for r in rows):
        subprocess.run([sys.executable, str(REPO / "scripts/stage18-split-entities.py")], cwd=REPO, check=True)
    # Accessibility is recomputed from the finished tree: what nothing else names becomes internal (D5).
    subprocess.run([sys.executable, str(REPO / "scripts/stage18-internal-sweep.py")], cwd=REPO, check=True)
    # Namespace rewrites reorder `using` blocks, so the copied tree is formatted the way the pre-commit hook expects.
    roots = sorted({r["refactor_path"].split("/")[0] for r in rows})
    return subprocess.run(["dotnet", "csharpier", "format", *roots], cwd=REPO, check=False).returncode


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:] or ["18.2"]))

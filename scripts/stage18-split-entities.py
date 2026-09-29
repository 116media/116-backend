#!/usr/bin/env python3
"""Split the entity files above 300 lines into a state file and behaviour partials (stage 18 D6).

Usage: scripts/stage18-split-entities.py

State — properties, collections, the private EF constructor, the factories and their private validation —
stays in `Entities/<X>.cs`. Each behaviour cluster becomes `Behaviors/<X>.<Cluster>.cs`, holding the same
`partial class`. Idempotent: run it after any copy, on whichever files still carry their behaviour.
"""

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
CONTENT = "src.refactor/modules/Content/Content/src/Content.Domain/Entities"
IDENTITY = "src.refactor/modules/Identity/Identity/src/Identity.Domain/Entities"

# entity file -> cluster -> the members that move there. Anything unlisted stays with the state.
CLUSTERS: dict[str, dict[str, tuple[str, ...]]] = {
    f"{CONTENT}/LyricsEntity.cs": {
        "Editorial": (
            "Retitle", "ReviseText", "Recategorize", "Relink", "AssignCommission", "ReviseSeo",
            "UpdateMetadata", "SetCoverImageFileId", "ReplaceLyricsText", "LinkArtist", "UnlinkArtist",
            "LinkAlbum", "UnlinkAlbum", "ReplaceTags",
        ),
        "Publication": ("Submit", "MarkPendingReview", "Approve", "Publish", "Reject", "Archive"),
        "Promotion": ("StampPromotion", "ForceUnpromote"),
        # Lyrics carries 30 documented properties, so its state file is still over 300 lines with the
        # behaviour gone; construction is the cluster that brings it under.
        "Creation": ("CreateFree", "CreatePaid", "ValidateRequiredFields"),
    },
    f"{CONTENT}/ArticleEntity.cs": {
        "Editorial": (
            "Retitle", "ReviseBody", "Recategorize", "AssignCommission", "UpdateCoverImage", "ReviseSeo",
            "AddImage", "RemoveCoverImage", "RemoveBodyImages", "ReplaceTags", "ReplaceArtists",
        ),
        "Publication": ("Submit", "MarkPendingReview", "Approve", "Publish", "Reject", "Archive", "MarkDeleted"),
        "Promotion": ("StampSocialBoost", "StampPromotion", "ForceUnpromote"),
    },
    f"{CONTENT}/VideoEntity.cs": {
        "Editorial": (
            "Retitle", "ReviseDescription", "Recategorize", "AssignCommission", "SetThumbnailFileId",
            "AttachYoutubeVideoUrl", "ScheduleShoot", "ReviseSeo", "LinkArtist", "UnlinkArtist", "ReplaceTags",
        ),
        "Publication": ("Submit", "MarkPendingReview", "Approve", "Publish", "Reject", "Archive", "MarkDeleted"),
        "Promotion": ("StampSocialBoost", "StampPromotion", "ForceUnpromote"),
    },
    f"{CONTENT}/CategoryEntity.cs": {
        "Catalogue": (
            "Rename", "Redescribe", "Reclassify", "MarkChanged", "SetDefaultForLyrics", "ClearDefaultForLyrics",
            "EnsureCommissionable", "Activate", "Deactivate", "SetPosterFileId", "SetExclusive", "ClearExclusive",
            "PinToFeed", "UnpinFromFeed",
        ),
        "Pricing": ("SetPricing", "RemovePricing", "FindPricing"),
    },
    f"{CONTENT}/ArtistEntity.cs": {
        "Profile": (
            "Rename", "ReviseProfile", "MarkChanged", "ReplaceAliases", "GuardBirthdate",
            "RecomputeNameIndexes", "FoldName", "SetAvatarFileId", "ClaimOwnership",
        ),
        "SocialLinks": ("SetSocialLink", "RemoveSocialLink", "FindSocialLink"),
    },
    f"{CONTENT}/ContentOrderEntity.cs": {
        "Composition": (
            "EnsureDraft", "Update", "AddItem", "AddItems", "AddTier", "RemoveTier", "RemoveItem", "FindItem",
            "RecalculateTotalFromItems",
        ),
        "Payment": (
            "AttachPayment", "RejectPayment", "Submit", "MarkPaid", "Cancel", "ResolvePromotionUntil",
            "TruncateToMilliseconds",
        ),
    },
    f"{IDENTITY}/UserEntity.cs": {
        "Credentials": (
            "LinkProviderSubject", "UpdateEmail", "InitializePasswordHash", "UpdatePassword",
            "SetPasswordAndChangeToLocal", "MarkAsVerified", "MarkVerifiedByOtp", "RecordMassSignOut",
            "ValidateCanLogin",
        ),
        "Profile": ("UpdateUserName", "UpdateAvatar", "UpdatePhoneNumber", "SetPreferredLocale"),
        "Access": ("Activate", "Deactivate", "GrantRole", "GrantInitialRole", "RevokeRole", "HasRole"),
    },
}

CLASS_DECLARATION_RX = re.compile(r"^public (?:sealed )?(?:partial )?class (\w+)", re.M)
MEMBER_NAME_RX = re.compile(
    r"^\s*(?:public|private|internal|protected)[\w\s<>,\[\]?.]*?\b(\w+)\s*(?:\(|=>|\{)"
)


def member_name(block: str) -> str | None:
    for line in block.splitlines():
        stripped = line.strip()
        if not stripped or stripped.startswith(("///", "[", "//")):
            continue
        match = MEMBER_NAME_RX.match(line)
        return match.group(1) if match else None
    return None


def split_members(body: str) -> list[str]:
    """The class body as a list of blocks, each a member with the doc comment and attributes above it."""
    blocks: list[str] = []
    current: list[str] = []
    depth = 0
    for line in body.splitlines(keepends=True):
        current.append(line)
        depth += line.count("{") - line.count("}")
        ended = depth == 0 and (line.rstrip().endswith(("}", ";")) or line.strip() == "}")
        if depth == 0 and ended and line.strip():
            blocks.append("".join(current))
            current = []
    if current:
        blocks.append("".join(current))
    return blocks


def behaviour_file(path: Path, header: str, class_name: str, cluster: str, blocks: list[str]) -> str:
    usings = "".join(line for line in header.splitlines(keepends=True) if line.startswith("using "))
    namespace = next(line for line in header.splitlines(keepends=True) if line.startswith("namespace "))
    doc = (
        f"/// <summary>\n"
        f"/// {cluster} behaviour of <see cref=\"{class_name}\" />. Its state lives in <c>Entities/{class_name}.cs</c>.\n"
        f"/// </summary>\n"
    )
    return f"{usings}\n{namespace}\n{doc}public sealed partial class {class_name}\n{{\n" + "\n".join(
        block.rstrip("\n") + "\n" for block in blocks
    ) + "}\n"


def split(relative: str, clusters: dict[str, tuple[str, ...]]) -> tuple[int, int]:
    path = REPO / relative
    text = path.read_text(encoding="utf-8")
    declaration = CLASS_DECLARATION_RX.search(text)
    if declaration is None:
        raise SystemExit(f"no class declaration in {relative}")
    class_name = declaration.group(1)

    open_brace = text.index("{", declaration.end())
    header = text[: declaration.start()]
    body = text[open_brace + 1 : text.rindex("}")]

    owner = {name: cluster for cluster, names in clusters.items() for name in names}
    kept: list[str] = []
    moved: dict[str, list[str]] = {cluster: [] for cluster in clusters}
    for block in split_members(body):
        cluster = owner.get(member_name(block) or "")
        (moved[cluster] if cluster else kept).append(block)

    if not any(moved.values()):
        return 0, 0

    behaviours = path.parent.parent / "Behaviors"
    behaviours.mkdir(exist_ok=True)
    for cluster, blocks in moved.items():
        if blocks:
            target = behaviours / f"{class_name}.{cluster}.cs"
            target.write_text(behaviour_file(target, header, class_name, cluster, blocks), encoding="utf-8")

    state_declaration = text[declaration.start() : open_brace].replace(
        f"class {class_name}", f"partial class {class_name}", 1
    )
    if "partial partial" in state_declaration:
        state_declaration = state_declaration.replace("partial partial", "partial")
    path.write_text(header + state_declaration + "{\n" + "".join(kept) + "}\n", encoding="utf-8")
    return len(kept), sum(len(blocks) for blocks in moved.values())


def main() -> int:
    for relative, clusters in CLUSTERS.items():
        kept, moved = split(relative, clusters)
        if moved:
            print(f"  {Path(relative).name}: {kept} state members kept, {moved} moved into {len(clusters)} partials")
    roots = sorted({relative.split("/")[0] for relative in CLUSTERS})
    return subprocess.run(["dotnet", "csharpier", "format", *roots], cwd=REPO, check=False).returncode


if __name__ == "__main__":
    sys.exit(main())

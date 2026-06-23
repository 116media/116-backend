# Category Poster Colors — Design

Design for deriving a **dominant background color** and a **contrasting
foreground (text) color** from a category's poster image and exposing them on
every category query as `Colors.Background` / `Colors.Foreground`. This is the
agreed design — implementation and tests follow.

---

## Goal

The frontend renders show cards over the category poster. Today the text color
is fixed, so a light poster (e.g. yellow) with white text is unreadable. We want
the backend to provide, per poster:

- **`Background`** — the **single most-dominant** color of the poster (hex
  `#RRGGBB`). Cloudinary returns several predominant colors ranked by coverage;
  the top one is always the background.
- **`Foreground`** — **not** a second extracted color. It is **computed** from
  `Background` (black or white, chosen by WCAG luminance) so it is guaranteed to
  contrast and the UI never has to guess.

So: one color extracted (the dominant ⇒ `Background`), one color derived (its
contrast partner ⇒ `Foreground`). Both are plain `#RRGGBB` hex, ready to drop
straight into CSS (`background-color` / `color`) on the frontend — no parsing.
They travel on the `CategoryDto` as a ready-to-use, accessible pair.

---

## Storage — compute once at write, store both

Both colors are computed **once, at upload time**, and stored on the file. The
read path (querying categories) is then a pure pass-through with **no
computation** — important when listing hundreds/thousands of categories. The
category itself stores no colors; the mapper already loads the poster
`FileEntity` (to resolve `PosterUrl`) and reads both colors from it.

### `core.files`

| column | type | null? | meaning |
|--------|------|-------|---------|
| `dominant_color_hex` | `varchar(7)` | yes | dominant color ⇒ `Background` |
| `foreground_color_hex` | `varchar(7)` | yes | computed contrast ⇒ `Foreground` |

`varchar(7)` for `#RRGGBB`. Nullable — both stay `null` until extraction runs
(existing files, or non-image files). These are two **distinct** values (not a
duplicated color), and they are generic image-level facts — reusable later for
thumbnails/avatars.

### What does NOT change

- **No `categories` columns**, no `CategoryColors` owned type, no Content
  migration.
- **`AdminUploadCategoryPoster` handler is unchanged** — it uploads and sets
  `PosterFileId` exactly as today; the colors are recorded by the upload
  pipeline automatically.

The earlier "store the same dominant color on both `files` and `categories`"
duplication is gone: each color is stored exactly once, on the file.

---

## Extraction pipeline (generic, all image uploads)

The poster already flows through Cloudinary on upload. We reuse that — **no new
image library** — and compute the contrast color in the same step.

### 1. Request the dominant color from Cloudinary

`src/Modules/Core/Core/Infrastructure/Services/CloudinaryService.cs` —
`UploadImageAsync` sets `Colors = true` on `ImageUploadParams`. Cloudinary
returns a predominant-colors palette (color + coverage %); take the single
most-dominant one and normalize to `#RRGGBB` (upper-case, validated). Surface it
on `CloudinaryUploadResult.DominantColorHex` (and the abstraction's
`FileUploadResult`).

> **Verify on implementation:** confirm the installed `CloudinaryDotNet`
> (v1.27.9) surfaces predominant colors on `ImageUploadResult` (e.g.
> `result.Colors` / `result.Predominant`). If the SDK doesn't expose it cleanly,
> fall back to adding `SixLabors.ImageSharp` and quantizing the stream locally.

### 2. Compute the foreground and store both on the file

In the generic image-store path
(`FileRepository.UploadAndStoreImageFileAsync` / `ReplaceImageFileAsync`):

```csharp
string? background = uploadResult.DominantColorHex;
string? foreground = background is { } bg ? ColorContrast.ForegroundFor(bg) : null;

var file = FileEntity.Create(
    // …existing args…
    dominantColorHex: background,
    foregroundColorHex: foreground
);
```

- `FileEntity` gains `DominantColorHex` + `ForegroundColorHex` properties and two
  optional `Create` parameters.
- `ColorContrast` runs **once here**, at write time. Because category posters go
  through this same path, they get both colors for free — no category-specific
  upload code.

---

## Foreground (contrast) algorithm

A small pure helper in the **Shared** module (so Core can call it at upload
time) — `src/Shared/Shared/Application/Common/ColorContrast.cs` (new),
unit-testable:

1. Parse `#RRGGBB` → sRGB channels in `[0,1]`.
2. Linearize each channel and compute **relative luminance**
   `L = 0.2126·R + 0.7152·G + 0.0722·B` (WCAG 2.x).
3. Pick the text color by the standard flip threshold:
   `L > 0.179 ? "#000000" : "#FFFFFF"` (dark text on light backgrounds, light
   text on dark) — this resolves the "yellow background → white text" problem
   (yellow is light ⇒ black text).

Black/white keeps it always-accessible and simple; a tinted variant can come
later if needed.

---

## DTO & mapping

`CategoryDto` gains an optional nested colors object; `CategoryMapper` reads both
colors from the poster file it already loads — **pure pass-through, no
computation**.

```csharp
public record CategoryColorsDto(string Background, string Foreground);

public record CategoryDto(
    // …existing fields…
    string? PosterUrl,          // already resolved from posterFile.StorageUrl
    CategoryColorsDto? Colors,  // null when no poster / no extracted color
    IReadOnlyList<CategoryPricingDto> Pricing
);
```

In every mapping path the mapper already has the poster `FileEntity` (to read
`StorageUrl`), so it reads both colors from the same object — **no extra query,
no per-card math**:

```csharp
CategoryColorsDto? colors =
    posterFile?.DominantColorHex is { } background && posterFile.ForegroundColorHex is { } foreground
        ? new CategoryColorsDto(background, foreground)
        : null;
```

This applies to all three mapper methods:
- `ToCategoryDtoAsync` (single — fetches the file by `PosterFileId`)
- `ToCategoryDtosAsync` (batch — per item)
- `ToCategoryDto` (bulk — file from the pre-loaded files dictionary)

So `Colors` rides **every** existing category response (public
`GET /api/v1/public/categories`, the feeds, admin lists, the poster-upload
response). No new endpoints; the `FileEntity` is never serialized out — clients
still see only `PosterUrl` + `Colors`. Listing 1000+ categories costs zero color
computation at read time.

---

## Persistence & migration

- **Core** (`CoreDbContext`) — one migration `AddFileColors`:
  `ALTER TABLE core.files ADD dominant_color_hex varchar(7) NULL, foreground_color_hex varchar(7) NULL`.
  Configure both properties in the `FileEntity` EF configuration.
- **Content** — **no migration** (no category schema change).

Purely additive and nullable, so safe on existing rows. Existing files keep both
columns `NULL` (so their categories return `Colors = null`) until a poster is
re-uploaded.

A one-off **backfill** (re-extract for existing posters) is **out of scope**
here. Note: because the foreground is stored, a future change to the contrast
rule would need a backfill to refresh stored foregrounds — a simple one-off job.

---

## Edge cases

- **No poster / article categories** → `Colors` is `null`; clients fall back to
  their default styling.
- **Extraction fails or returns nothing** → both columns stay `null`; never block
  the upload on color analysis.
- **Color format** → always `#RRGGBB` upper-case; rejected/ignored otherwise.
- **Idempotency** → re-uploading a poster recomputes and overwrites both colors.

---

## Test plan (after implementation)

**Unit**
- `ColorContrast.ForegroundFor`: known inputs → expected text color (yellow
  `#FFEB3B` → `#000000`; navy `#0D1B2A` → `#FFFFFF`; mid-grays around the
  threshold; pure black/white).
- Hex normalization/validation at extraction (valid → upper-cased `#RRGGBB`;
  invalid → ignored / `null`).
- `CategoryMapper`: poster file with colors → `Colors` populated as a
  pass-through; no poster / no colors → `Colors` null.

**Integration**
- Upload a poster → the file persists both `dominant_color_hex` and
  `foreground_color_hex`; the response `CategoryDto.Colors` is populated and
  contrasting.
- `GET /api/v1/public/categories` → returns `Colors` for categories with a
  poster, `null` otherwise.
- Replacing a poster updates both colors.

---

## File inventory

**New**
- `Shared/Application/Common/ColorContrast.cs`
- `Content/Application/Shared/DTOs/CategoryColorsDto.cs` (or alongside `CategoryDto`)
- Migration: Core `AddFileColors`

**Modified**
- `Core/Infrastructure/Services/CloudinaryService.cs` (request colors, extract predominant)
- `Core/Application/Shared/Services/ICloudinaryService.cs` (`CloudinaryUploadResult.DominantColorHex`)
- `Core/.../FileUploadResult` + `FileRepository` (compute foreground, persist both)
- `Core/Domain/Entities/FileEntity.cs` (+ `DominantColorHex`, `ForegroundColorHex`, `Create` params) + its EF config
- `Content/Application/Shared/DTOs/CategoryDto.cs` (+ `Colors`)
- `Content/Application/Shared/Mappers/CategoryMapper.cs` (pass the poster file's colors through)

**Unchanged (notably)**
- `CategoryEntity`, `CategoryConfiguration`, and `AdminUploadCategoryPosterHandler`
  — no category schema or handler changes.

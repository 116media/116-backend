: # Editorial Validation Fix Plan

## The Standard (Identity + Content Catalog pattern)

All validators in this codebase follow a two-layer pattern:

### Layer 1 — Shared validation extension class
Located in `Application/Shared/Validators/` (Content) or `Application/Auth/Validators/` (Identity).

Each file is a `static partial class XxxValidation` containing `IRuleBuilder` extension methods.

**Rules for every extension method:**
- Full XML docs: `<summary>`, `<typeparam>`, `<param>`, `<returns>`
- Every rule has a `.WithMessage("...")` with a human-readable sentence
- All length/size values come from constants (never hardcoded magic numbers)
- `isRequired` bool parameter — same method handles both required and optional fields
- `CascadeMode.Stop` on fields where later rules are pointless if earlier ones fail
- `[GeneratedRegex]` for compiled regex (slug, etc.)
- `ValidationUtils.GetPropertyValue` used in `.When()` for optional field guard

**Reference files:**
- `src/Modules/Identity/Identity/Application/Auth/Validators/CredentialValidation.cs`
- `src/Modules/Identity/Identity/Application/Auth/Validators/ProfileValidation.cs`
- `src/Modules/Identity/Identity/Application/Auth/Validators/FileValidation.cs`
- `src/Modules/Content/Content/Application/Shared/Validators/CategoryValidation.cs`

### Layer 2 — Validator class (thin wrapper)
```csharp
public class CreateArticleValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleValidator()
    {
        RuleFor(x => x.CategoryId).ValidArticleCategoryId();
        RuleFor(x => x.Title).ValidArticleTitle();
        RuleFor(x => x.Slug).ValidArticleSlug();
    }
}
```
No inline magic numbers, no hardcoded messages — just calls to shared extensions.

---

## What Needs to Be Created

### New file: `Application/Shared/Validators/EditorialValidation.cs`

`static partial class EditorialValidation` with extensions for all editorial fields:

| Method | Field | Constants | Notes |
|---|---|---|---|
| `ValidArticleCategoryId` | `CategoryId (Guid)` | — | `NotEmpty` + message |
| `ValidArticleTitle` | `Title (string)` | `MaxTitleLength = 200` | `NotEmpty` + `MaxLength` |
| `ValidArticleSlug` | `Slug (string)` | `MaxSlugLength = 220` | `NotEmpty` + `MaxLength` + `SlugRegex` |
| `ValidArticleHeadline` | `Headline (string)` | `MinHeadlineLength = 100`, `MaxHeadlineLength = 300` | `NotEmpty` + `MinLength` + `MaxLength` |
| `ValidArticleBody` | `Body (string)` | — | `NotEmpty` + message |
| `ValidRejectionReason` | `Reason (string)` | `MaxRejectionReasonLength = 500` | `NotEmpty` + `MaxLength` |
| `ValidYoutubeVideoId` | `YoutubeVideoId (string)` | `MaxYoutubeVideoIdLength = 20` | `NotEmpty` + `MaxLength` |
| `ValidVideoTitle` | `Title (string)` | `MaxTitleLength = 200` | same as article title |
| `ValidVideoSlug` | `Slug (string)` | `MaxSlugLength = 220` | same as article slug |
| `ValidShortVideoTitle` | `Title (string)` | `MaxShortVideoTitleLength = 200` | `NotEmpty` + `MaxLength` |
| `ValidShortVideoFile` | `VideoFile (IFormFile)` | — | `NotNull` + message (follow `FileValidation` pattern) |
| `ValidSongTitle` | `SongTitle (string)` | `MaxSongTitleLength = 200` | `NotEmpty` + `MaxLength` |
| `ValidArtistName` | `ArtistName (string)` | `MaxArtistNameLength = 100` | `NotEmpty` + `MaxLength` |
| `ValidLyricsText` | `LyricsText (string)` | — | `NotEmpty` + message |
| `ValidLyricsLanguage` | `Language (string)` | `MaxLyricsLanguageLength = 5` | `NotEmpty` + `MaxLength` |
| `ValidShootingScheduledAt` | `ShootingScheduledAt (DateTimeOffset)` | — | `GreaterThan(UtcNow)` + message with field name |
| `ValidArticleImageFile` | `File (IFormFile)` | — | `NotNull` + message |
| `ValidArticleId` | `ArticleId (Guid)` | — | `NotEmpty` + message |

Slug methods need `[GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]` (same as `CategoryValidation`).

---

## The 11 Validator Files to Fix

All located under `Application/Editorial/UseCases/Admin/Commands/`:

### 1. `CreateArticle/CreateArticleValidator.cs`
**Current problems:** `MaximumLength(200)`, `MaximumLength(220)` hardcoded; no `WithMessage` on any rule.
**Fix:**
```csharp
RuleFor(x => x.CategoryId).ValidArticleCategoryId();
RuleFor(x => x.Title).ValidArticleTitle();
RuleFor(x => x.Slug).ValidArticleSlug();
RuleFor(x => x.OrderItemId)
    .NotEmpty()
    .When(x => x.CustomerId.HasValue)
    .WithMessage("Order item ID is required when customer ID is provided.");
RuleFor(x => x.CustomerId)
    .NotEmpty()
    .When(x => x.OrderItemId.HasValue)
    .WithMessage("Customer ID is required when order item ID is provided.");
```

### 2. `UpdateArticle/UpdateArticleValidator.cs`
**Current problems:** Uses `ContentConstants` ✓ but no `WithMessage` on any rule.
**Fix:**
```csharp
RuleFor(x => x.Id).NotEmpty().WithMessage("Article ID is required.");
RuleFor(x => x.Headline).ValidArticleHeadline();
RuleFor(x => x.Body).ValidArticleBody();
```

### 3. `RejectArticle/RejectArticleValidator.cs`
**Current problems:** `MaximumLength(500)` hardcoded; no `WithMessage` on length.
**Fix:**
```csharp
RuleFor(x => x.Reason).ValidRejectionReason();
```

### 4. `UploadArticleImage/UploadArticleImageValidator.cs`
**Current problems:** No `WithMessage` on either rule.
**Fix:**
```csharp
RuleFor(x => x.ArticleId).ValidArticleId();
RuleFor(x => x.File).ValidArticleImageFile();
```

### 5. `CreateVideo/CreateVideoValidator.cs`
**Current problems:** Same as `CreateArticleValidator` — `MaximumLength(200)`, `MaximumLength(220)` hardcoded; no `WithMessage`.
**Fix:**
```csharp
RuleFor(x => x.CategoryId).ValidArticleCategoryId();
RuleFor(x => x.Title).ValidVideoTitle();
RuleFor(x => x.Slug).ValidVideoSlug();
RuleFor(x => x.OrderItemId)
    .NotEmpty()
    .When(x => x.CustomerId.HasValue)
    .WithMessage("Order item ID is required when customer ID is provided.");
RuleFor(x => x.CustomerId)
    .NotEmpty()
    .When(x => x.OrderItemId.HasValue)
    .WithMessage("Customer ID is required when order item ID is provided.");
```

### 6. `UpdateVideo/UpdateVideoValidator.cs`
**Current problems:** `MaximumLength(200)`, `MaximumLength(220)` hardcoded; no `WithMessage`.
**Fix:**
```csharp
RuleFor(x => x.Id).NotEmpty().WithMessage("Video ID is required.");
RuleFor(x => x.Title).ValidVideoTitle();
RuleFor(x => x.Slug).ValidVideoSlug();
```

### 7. `RejectVideo/RejectVideoValidator.cs`
**Current problems:** `MaximumLength(500)` hardcoded; no `WithMessage` on length.
**Fix:**
```csharp
RuleFor(x => x.Reason).ValidRejectionReason();
```

### 8. `AttachYoutubeId/AttachYoutubeIdValidator.cs`
**Current problems:** `MaximumLength(20)` hardcoded; no `WithMessage`.
**Fix:**
```csharp
RuleFor(x => x.YoutubeVideoId).ValidYoutubeVideoId();
```

### 9. `ScheduleShoot/ScheduleShootValidator.cs`
**Current problems:** Message `"must be in the future"` missing field name prefix.
**Fix:**
```csharp
RuleFor(x => x.ShootingScheduledAt).ValidShootingScheduledAt();
```

### 10. `CreateShortVideo/CreateShortVideoValidator.cs`
**Current problems:** `MaximumLength(200)` hardcoded; `WithMessage` only on `NotNull`, missing on length.
**Fix:**
```csharp
RuleFor(x => x.Title).ValidShortVideoTitle();
RuleFor(x => x.VideoFile).ValidShortVideoFile();
```

### 11. `CreateLyrics/CreateLyricsValidator.cs`
**Current problems:** `MaximumLength(200)`, `MaximumLength(100)`, `MaximumLength(5)` all hardcoded; no `WithMessage` on length rules.
**Fix:**
```csharp
RuleFor(x => x.SongTitle).ValidSongTitle();
RuleFor(x => x.ArtistName).ValidArtistName();
RuleFor(x => x.LyricsText).ValidLyricsText();
RuleFor(x => x.Language).ValidLyricsLanguage();
```

### 12. `UpdateLyrics/UpdateLyricsValidator.cs`
**Current problems:** No `WithMessage` on either rule.
**Fix:**
```csharp
RuleFor(x => x.Id).NotEmpty().WithMessage("Lyrics ID is required.");
RuleFor(x => x.LyricsText).ValidLyricsText();
```

---

## Implementation Order

1. Create `Application/Shared/Validators/EditorialValidation.cs`
2. Fix all 12 validator files (write each once)
3. Run `dotnet build` — zero errors expected (no API surface changes, only validation internals)

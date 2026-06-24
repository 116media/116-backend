# 07 — Error Messages

Three new error factory methods support the `pin-to-feed` handler guards. They follow the
exact shape of the existing `CannotMakeInactiveExclusive` / `OnlyVideoCategoryCanBeExclusive`
methods added by the exclusive-category work.

## CategoryErrors

**File:** `src/Modules/Content/Content/Application/Shared/Errors/CategoryErrors.cs`

```csharp
/// <summary>
/// Throws when attempting to pin an inactive category to the content feed.
/// </summary>
public BadRequestException CannotPinInactiveToFeed()
{
    return new BadRequestException(i18n.CannotPinInactiveToFeed());
}

/// <summary>
/// Throws when attempting to pin a category whose content type cannot appear
/// in a feed (only Video and Article are feedable).
/// </summary>
public BadRequestException ContentTypeNotFeedable()
{
    return new BadRequestException(i18n.ContentTypeNotFeedable());
}

/// <summary>
/// Throws when attempting to pin a category that does not have the minimum number
/// of published videos required to appear as a feed section.
/// </summary>
public BadRequestException NotEnoughVideosToPinToFeed()
{
    return new BadRequestException(i18n.NotEnoughVideosToPinToFeed());
}
```

## CategoryErrorMessage

**File:** `src/Modules/Content/Content/Application/Shared/Errors/Messages/CategoryErrorMessage.cs`

```csharp
/// <summary>
/// Message when trying to pin an inactive category to the feed.
/// </summary>
public string CannotPinInactiveToFeed()
{
    return _localizer["Category.CannotPinInactiveToFeed"];
}

/// <summary>
/// Message when trying to pin a category of a non-feedable content type.
/// </summary>
public string ContentTypeNotFeedable()
{
    return _localizer["Category.ContentTypeNotFeedable"];
}

/// <summary>
/// Message when trying to pin a category with too few published videos.
/// </summary>
public string NotEnoughVideosToPinToFeed()
{
    return _localizer["Category.NotEnoughVideosToPinToFeed"];
}
```

## Resource Files

Add the keys to every locale resource file (match the existing `Category.*` entries):

| Key | EN | FR |
| --- | --- | --- |
| `Category.CannotPinInactiveToFeed` | `An inactive category cannot be pinned to the feed.` | `Une catégorie inactive ne peut pas être épinglée au fil.` |
| `Category.ContentTypeNotFeedable` | `Only video and article categories can be pinned to the feed.` | `Seules les catégories de vidéos et d'articles peuvent être épinglées au fil.` |
| `Category.NotEnoughVideosToPinToFeed` | `This category needs at least 4 published videos to be pinned to the feed.` | `Cette catégorie doit avoir au moins 4 vidéos publiées pour être épinglée au fil.` |

Resource file locations (mirror the exclusive-category keys):

- `src/.../Resources/en/CategoryErrorMessage.en.resx`
- `src/.../Resources/fr/CategoryErrorMessage.fr.resx`

> Confirm the exact resx folder/naming against the existing
> `Category.CannotMakeInactiveExclusive` entries before adding — match whatever those use.

## What DOES throw

- **Pinning a category with fewer than `MinVideosToPinToFeed` published videos** — throws
  `NotEnoughVideosToPinToFeed`. This is the new eligibility gate; the count is over
  **published** videos only.

## What does NOT throw

- **FIFO eviction** — silently unpins the oldest category. No error, by design.
- **Unpinning a category that is not pinned** — idempotent no-op `200`. No error.
- **Hitting the cap** — never surfaced to the admin as an error; the cap is maintained
  automatically via eviction.

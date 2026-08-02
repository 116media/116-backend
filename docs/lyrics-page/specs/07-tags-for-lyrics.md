# Spec 07 — Tags for Lyrics

Lands before spec 06 (similar lyrics), which depends on it. `TagEntity`
(`Domain/Entities/TagEntity.cs`) already exists and already backs `ArticleTagEntity`/
`VideoTagEntity` — this spec adds the third join, `LyricsTagEntity`, reusing the same tag pool
(an "Afrobeat" tag applied to an article or video is the same row a lyrics page applies). No new
tag CRUD, no new tag admin UI — only the join.

## `LyricsTagEntity`

Direct copy of `ArticleTagEntity` (`Domain/Entities/ArticleTagEntity.cs`):

```csharp
/// <summary>
/// Junction entity linking a lyrics page to a tag (many-to-many).
/// </summary>
public class LyricsTagEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identifier of the lyrics page.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; private set; }

    /// <summary>
    /// The lyrics page associated with this tag relationship.
    /// </summary>
    public LyricsEntity Lyrics { get; private set; } = null!;

    /// <summary>
    /// The tag associated with this lyrics relationship.
    /// </summary>
    public TagEntity Tag { get; private set; } = null!;

    private LyricsTagEntity() { }

    /// <summary>
    /// Creates a new lyrics-tag association.
    /// </summary>
    public static LyricsTagEntity Create(Guid id, Guid lyricsId, Guid tagId)
    {
        return new LyricsTagEntity { Id = id, LyricsId = lyricsId, TagId = tagId, CreatedAt = DateTime.UtcNow };
    }
}
```

`LyricsEntity` gains the navigation collection, mirroring `VideoEntity.Tags`:

```csharp
/// <summary>
/// Tags applied to this lyrics page for discovery and similar-lyrics matching.
/// </summary>
public ICollection<LyricsTagEntity> Tags { get; } = new List<LyricsTagEntity>();
```

## Configuration

```csharp
public class LyricsTagConfiguration : IEntityTypeConfiguration<LyricsTagEntity>
{
    public void Configure(EntityTypeBuilder<LyricsTagEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LyricsId).IsRequired();
        builder.Property(x => x.TagId).IsRequired();
        builder.HasIndex(x => new { x.LyricsId, x.TagId }).IsUnique();

        builder.HasOne(x => x.Lyrics).WithMany(l => l.Tags).HasForeignKey(x => x.LyricsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## Setting tags — one replace-the-set method, not incremental add/remove

Mirrors how `VideoEntity`'s tag set is managed at the application layer (the entity itself stays
simple; the handler diffs and replaces):

```csharp
/// <summary>
/// Command to replace the full set of tags applied to a lyrics page.
/// </summary>
/// <param name="LyricsId">The lyrics page to tag.</param>
/// <param name="TagIds">The complete new set of tag identifiers. An empty array clears all tags.</param>
public record AdminSetLyricsTagsCommand(Guid LyricsId, IReadOnlyCollection<Guid> TagIds)
    : ICommand<AdminSetLyricsTagsResult>;
```

```csharp
public class AdminSetLyricsTagsHandler(
    ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminSetLyricsTagsCommand, AdminSetLyricsTagsResult>
{
    public async Task<AdminSetLyricsTagsResult> Handle(AdminSetLyricsTagsCommand command, CancellationToken ct)
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(command.LyricsId, ct);

        await lyricsRepository.ReplaceTagsAsync(
            lyricsId: command.LyricsId, tagIds: command.TagIds, cancellationToken: ct);

        return new AdminSetLyricsTagsResult(IsSuccess: true);
    }
}
```

`ILyricsRepository.ReplaceTagsAsync(Guid lyricsId, IReadOnlyCollection<Guid> tagIds, ...)` removes
every existing `LyricsTagEntity` row for that lyrics id and inserts the new set in one call —
simpler than a diff, and correct because tag sets are small (a handful of tags per song, never a
large collection needing incremental updates).

Route: `PUT /api/v1/admin/lyrics/{id}/tags`, mirroring the existing
`EditorialRouteConstants.Tags` segment already used for articles/videos.

## `LyricsDto` addition

```csharp
public record LyricsDto(
    /* ...existing fields... */,
    IReadOnlyList<string> TagNames
) : AuditableDto;
```

Populated from `entity.Tags.Select(t => t.Tag.Name)` — display-only; the frontend never edits tags
directly, only the dashboard does.

## Migration

```bash
dotnet ef migrations add AddLyricsTags \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `LyricsTagEntity` + `LyricsTagConfiguration`
- [x] `LyricsEntity.Tags` navigation collection
- [x] `ILyricsRepository.ReplaceTagsAsync`
- [x] `AdminSetLyricsTagsCommand`/`Handler`/`EndpointV1` (`PUT /api/v1/admin/lyrics/{id}/tags`)
- [x] `LyricsDetailDto.Tags: IReadOnlyList<TagDto>` — NOT `TagNames: string[]` as this doc
  originally sketched; `ArticleDetailDto.Tags` is the real precedent, so lyrics matches it exactly.
  Populated only on single-item detail reads (`GetByIdAsync`/`GetByIdOrThrowAsync`/
  `GetBySlugAsync`/`GetByVideoIdAsync`, via `.Include(Tags).ThenInclude(Tag)`) — deliberately NOT
  added to the paginated `GetAllAsync`/`LyricsSummaryDto`, since list cards don't render tags and
  the extra join isn't worth paying on every list page
- [x] Migration `AddLyricsTags`
- [x] Integration tests: setting an empty array clears all tags; setting a new set fully replaces
  the old one (no leftover rows); the same `TagEntity` can be applied to an article, a video, and a
  lyrics page simultaneously without conflict

**Verification, 2026-07-30**: `dotnet build` clean; covered by the same Lyrics-scoped test run as
spec 03 (264/264 unit, 112/112 integration, zero skips) since both specs shipped in the same
implementation pass.

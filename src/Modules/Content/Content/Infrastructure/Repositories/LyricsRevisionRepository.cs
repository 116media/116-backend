using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ILyricsRevisionRepository" /> for managing lyrics-text
/// community correction revision entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LyricsRevisionRepository(ContentDbContext context)
    : ContentRepository<LyricsRevisionEntity>(context),
        ILyricsRevisionRepository { }

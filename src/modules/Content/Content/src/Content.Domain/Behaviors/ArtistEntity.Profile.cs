using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Profile behaviour of <see cref="ArtistEntity" />. Its state lives in <c>Entities/ArtistEntity.cs</c>.
/// </summary>
public sealed partial class ArtistEntity
{
    /// <summary>
    /// Renames the artist and recomputes the folded-name and initial-letter indexes the
    /// directory browses by. Slug is immutable after creation to preserve public URLs.
    /// </summary>
    /// <param name="name">The artist's display name.</param>
    /// <returns><c>true</c> if the name changed; otherwise <c>false</c>.</returns>
    public bool Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistNameRequired);
        }

        if (Name == name)
        {
            return false;
        }

        Name = name;

        RecomputeNameIndexes();
        MarkChanged();

        return true;
    }

    /// <summary>
    /// Revises the biographical fields. The alias list is normalised on the way in, so an
    /// incoming list that normalises to the current one writes nothing.
    /// </summary>
    /// <param name="bio">Optional free-text biography, or null to clear it.</param>
    /// <param name="realName">The artist's legal or birth name, or null to clear it.</param>
    /// <param name="aliases">Alternate names, or null to clear them.</param>
    /// <param name="birthdate">The artist's date of birth, or null to clear it.</param>
    /// <param name="hometown">Where the artist is from, or null to clear it.</param>
    /// <param name="today">The current date, against which a future birthdate is rejected.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool ReviseProfile(
        string? bio,
        string? realName,
        IReadOnlyList<string>? aliases,
        DateOnly? birthdate,
        string? hometown,
        DateOnly today
    )
    {
        GuardBirthdate(birthdate: birthdate, today: today);

        List<string> currentAliases = [.. _aliases];
        ReplaceAliases(aliases: aliases);

        bool aliasesChanged = !_aliases.SequenceEqual(currentAliases, StringComparer.Ordinal);

        if (!aliasesChanged && Bio == bio && RealName == realName && Birthdate == birthdate && Hometown == hometown)
        {
            return false;
        }

        Bio = bio;
        RealName = realName;
        Birthdate = birthdate;
        Hometown = hometown;

        MarkChanged();

        return true;
    }

    /// <summary>
    /// Records one change notice per unit of work, however many edit verbs the handler calls.
    /// </summary>
    private void MarkChanged()
    {
        if (DomainEvents.OfType<ArtistChangedEvent>().Any())
        {
            return;
        }

        AddDomainEvent(new ArtistChangedEvent(ArtistId: Id));
    }

    /// <summary>
    /// Normalises and stores the alias list: trimmed, blanks dropped, de-duplicated
    /// case-insensitively, then bounded by count and length. Normalising here rather than in
    /// a validator means the invariant holds for every writer, including seeds.
    /// </summary>
    /// <param name="aliases">The incoming aliases, or null for none.</param>
    private void ReplaceAliases(IReadOnlyList<string>? aliases)
    {
        _aliases.Clear();

        if (aliases is null)
        {
            return;
        }

        var seen = new HashSet<string>(comparer: StringComparer.OrdinalIgnoreCase);

        foreach (string alias in aliases)
        {
            string trimmed = alias?.Trim() ?? string.Empty;

            if (trimmed.Length == 0 || !seen.Add(item: trimmed))
            {
                continue;
            }

            if (trimmed.Length > ContentConstants.MaxArtistNameLength)
            {
                throw new ContentRuleException(ContentRuleCodes.ArtistAliasTooLong);
            }

            _aliases.Add(item: trimmed);
        }

        if (_aliases.Count > ContentConstants.MaxArtistAliasCount)
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistTooManyAliases);
        }
    }

    /// <summary>
    /// Rejects a birthdate that is not strictly in the past. A future birthdate is bad data,
    /// not a value the profile should render with a negative age.
    /// </summary>
    /// <param name="birthdate">The birthdate to validate, or null.</param>
    private static void GuardBirthdate(DateOnly? birthdate, DateOnly today)
    {
        if (birthdate is not null && birthdate.Value >= today)
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistBirthdateInFuture);
        }
    }

    /// <summary>
    /// Recomputes <see cref="NameFolded" /> and <see cref="InitialLetter" /> from the current
    /// <see cref="Name" />.
    /// </summary>
    private void RecomputeNameIndexes()
    {
        NameFolded = FoldName(name: Name);

        InitialLetter =
            NameFolded.Length > 0 && NameFolded[0] is >= 'A' and <= 'Z'
                ? NameFolded[..1]
                : ContentConstants.NonAlphabeticLetterBucket;
    }

    /// <summary>
    /// Strips diacritics, collapses whitespace and upper-cases a display name so that
    /// "Élodie" and "elodie" fold to the same sortable, searchable key.
    /// </summary>
    /// <param name="name">The display name to fold.</param>
    /// <returns>The folded form, safe to sort, bucket and search on.</returns>
    public static string FoldName(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            return string.Empty;
        }

        string collapsed = Regex.Replace(input: name.Trim(), pattern: @"\s+", replacement: " ");
        string decomposed = collapsed.Normalize(normalizationForm: NormalizationForm.FormD);

        var builder = new StringBuilder(capacity: decomposed.Length);

        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch: character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(value: character);
            }
        }

        return builder.ToString().Normalize(normalizationForm: NormalizationForm.FormC).ToUpperInvariant();
    }

    /// <summary>
    /// Sets or clears the avatar file reference.
    /// </summary>
    /// <param name="avatarFileId">The FileEntity ID, or null to clear it.</param>
    public void SetAvatarFileId(Guid? avatarFileId)
    {
        AvatarFileId = avatarFileId;
        AddDomainEvent(new ArtistChangedEvent(ArtistId: Id));
    }

    /// <summary>
    /// Links this profile to a verified artist account. One profile can be claimed by
    /// exactly one account — enforced here and by a database unique constraint on
    /// <c>UserId</c>.
    /// </summary>
    /// <param name="userId">The identity user UUID claiming this profile.</param>
    /// <exception cref="ContentRuleException">
    /// Thrown if the profile is already claimed.
    /// </exception>
    public void ClaimOwnership(Guid userId, DateTimeOffset now)
    {
        if (UserId.HasValue)
        {
            throw new ContentRuleException(ContentRuleCodes.ArtistAlreadyClaimed);
        }

        UserId = userId;
        VerifiedAt = now;

        AddDomainEvent(new ArtistOwnershipVerifiedEvent(ArtistId: Id, UserId: userId));
    }
}

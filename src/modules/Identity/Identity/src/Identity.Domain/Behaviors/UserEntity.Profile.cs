using System.ComponentModel.DataAnnotations;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Domain;
using _116.Shared.Domain.Constants;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Profile behaviour of <see cref="UserEntity" />. Its state lives in <c>Entities/UserEntity.cs</c>.
/// </summary>
public sealed partial class UserEntity
{
    /// <summary>
    /// Updates the username. Must still be unique across all users.
    /// </summary>
    public void UpdateUserName(string newUserName)
    {
        if (string.IsNullOrWhiteSpace(value: newUserName) || newUserName.Length > UserConstants.MaxUserNameLength)
        {
            throw new IdentityRuleException(IdentityRuleCodes.InvalidUsernameFormat, newUserName);
        }

        UserName = newUserName;
    }

    // Profile Methods
    /// <summary>
    /// Updates or removes the user's avatar.
    /// </summary>
    public void UpdateAvatar(Guid? avatarFileId, EnumAvatarSource avatarSource)
    {
        AvatarFileId = avatarFileId;
        AvatarSource = avatarSource;
    }

    /// <summary>
    /// Updates the user's phone number and country information.
    /// Pass nulls to clear the phone number.
    /// </summary>
    public void UpdatePhoneNumber(
        string? countryName,
        string? countryIsoCode,
        string? countryDialCode,
        string? fullPhoneNumber,
        string? partialPhoneNumber
    )
    {
        CountryName = countryName;
        CountryIsoCode = countryIsoCode;
        CountryDialCode = countryDialCode;
        FullPhoneNumber = fullPhoneNumber;
        PartialPhoneNumber = partialPhoneNumber;
    }

    /// <summary>
    /// Sets the locale this user's messages render in. No-ops when unchanged.
    /// </summary>
    /// <param name="locale">The two-letter locale code.</param>
    /// <returns>True when the stored locale changed.</returns>
    public bool SetPreferredLocale(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale) || PreferredLocale == locale)
        {
            return false;
        }

        PreferredLocale = locale;

        return true;
    }
}

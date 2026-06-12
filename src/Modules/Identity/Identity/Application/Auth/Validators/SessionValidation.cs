using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for session-related validation.
/// </summary>
public static class SessionValidation
{
    /// <summary>
    /// Validates that a refresh token is provided and not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the refresh token property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidRefreshToken<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.RefreshTokenRequired());
    }

    /// <summary>
    /// Validates that the export format is a supported value (Csv or Xlsx).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the format property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidExportFormat<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder
            .Must(format =>
                !string.IsNullOrWhiteSpace(value: format)
                && Enum.TryParse<EnumSessionExportFormat>(value: format, ignoreCase: true, result: out _)
            )
            .WithMessage(i18n.ExportFormatInvalid());
    }

    /// <summary>
    /// Validates that all provided column names are present in the allowed set.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the columns property.</param>
    /// <param name="validColumns">The full set of allowed column names.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidExportColumns<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string[] validColumns,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder
            .Must(columns =>
            {
                if (string.IsNullOrWhiteSpace(value: columns))
                {
                    return true;
                }

                string[] columnList = columns.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );

                return columnList.All(column =>
                    validColumns.Contains(value: column, comparer: StringComparer.OrdinalIgnoreCase)
                );
            })
            .WithMessage(i18n.ExportColumnsInvalid(string.Join(", ", validColumns)));
    }

    /// <summary>
    /// Validates that the session status filter is either 'active' or 'expired'.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the status property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidExportStatus<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder
            .Must(status =>
                !string.IsNullOrWhiteSpace(value: status)
                && Enum.TryParse<EnumSessionStatus>(value: status, ignoreCase: true, result: out _)
            )
            .WithMessage(i18n.ExportStatusInvalid());
    }
}

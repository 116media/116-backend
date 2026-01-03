using _116.Identity.Domain.Enums;

using FluentValidation;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Validator for <see cref="AdminExportSessionDataQuery" /> that ensures valid filter parameters.
/// </summary>
public class AdminExportSessionDataValidator : AbstractValidator<AdminExportSessionDataQuery>
{
    private static readonly string[] ValidColumns = typeof(SessionExportDto)
        .GetProperties()
        .Select(p => p.Name)
        .ToArray();

    /// <summary>
    /// Configure validation rules for the export query parameters.
    /// </summary>
    public AdminExportSessionDataValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(value: x.Format), () =>
        {
            RuleFor(x => x.Format)
                .Must(predicate: BeValidExportFormat)
                .WithMessage("Format must be one of: Csv, Xlsx.");
        });

        When(x => !string.IsNullOrWhiteSpace(value: x.Columns), () =>
        {
            RuleFor(x => x.Columns)
                .Must(predicate: BeValidColumns)
                .WithMessage(
                    $"Invalid column name(s). Valid columns are: {string.Join<string>(", ", values: ValidColumns)}."
                );
        });

        When(x => !string.IsNullOrWhiteSpace(value: x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(predicate: BeValidSessionStatus)
                .WithMessage("Status must be either 'active' or 'expired'.");
        });

        When(x => x.FromDate.HasValue && x.ToDate.HasValue, () =>
        {
            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate)
                .WithMessage("ToDate must be greater than or equal to FromDate.");
        });
    }

    /// <summary>
    /// Validates that the provided format is a valid export format.
    /// </summary>
    /// <param name="format">The string format to validate.</param>
    /// <returns>True if the format is valid, otherwise false.</returns>
    private static bool BeValidExportFormat(string? format)
    {
        return !string.IsNullOrWhiteSpace(value: format) &&
               Enum.TryParse<SessionExportFormat>(value: format, true, result: out _);
    }

    /// <summary>
    /// Validates that the provided status is a valid session status.
    /// </summary>
    /// <param name="status">The string status to validate.</param>
    /// <returns>True if the status is valid, otherwise false.</returns>
    private static bool BeValidSessionStatus(string? status)
    {
        return !string.IsNullOrWhiteSpace(value: status) &&
               Enum.TryParse<EnumSessionStatus>(value: status, true, result: out _);
    }

    /// <summary>
    /// Validates that all provided column names are valid.
    /// </summary>
    /// <param name="columns">Comma-separated column names.</param>
    /// <returns>True if all column names are valid, otherwise false.</returns>
    private static bool BeValidColumns(string? columns)
    {
        if (string.IsNullOrWhiteSpace(value: columns))
        {
            return true;
        }

        string[] columnList =
            columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return columnList.All(column =>
            ValidColumns.Contains(value: column, comparer: StringComparer.OrdinalIgnoreCase)
        );
    }
}

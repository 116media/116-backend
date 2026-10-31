using _116.BuildingBlocks.Application.Pagination;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Validates the public search query: a non-empty term of 2 to 120 characters and a
/// scope within the supported enum. Pagination is not validated here because
/// <see cref="PaginatedRequest" /> already clamps both of its values in its constructor.
/// </summary>
public class PublicSearchValidator : AbstractValidator<PublicSearchQuery>
{
    private const int MinLength = 2;
    private const int MaxLength = 120;

    /// <summary>
    /// Configures the rules for <see cref="PublicSearchQuery" />.
    /// </summary>
    public PublicSearchValidator()
    {
        RuleFor(query => query.Query).NotEmpty().MinimumLength(MinLength).MaximumLength(MaxLength);

        RuleFor(query => query.Scope).IsInEnum();
    }
}

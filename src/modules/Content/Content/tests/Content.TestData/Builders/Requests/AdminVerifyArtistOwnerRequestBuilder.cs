using _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminVerifyArtistOwnerRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminVerifyArtistOwnerRequestBuilder
{
    private Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminVerifyArtistOwnerRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminVerifyArtistOwnerRequestBuilder()
    {
        _userId = Guid.NewGuid();
    }

    /// <summary>
    /// Sets the user id.
    /// </summary>
    /// <param name="userId">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminVerifyArtistOwnerRequestBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminVerifyArtistOwnerRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminVerifyArtistOwnerRequest instance.</returns>
    public AdminVerifyArtistOwnerRequest Build()
    {
        return new AdminVerifyArtistOwnerRequest(UserId: _userId);
    }
}

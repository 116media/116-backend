using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminDecideTranslationRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminDecideTranslationRevisionRequestBuilder
{
    private bool _accept;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDecideTranslationRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminDecideTranslationRevisionRequestBuilder()
    {
        _accept = true;
    }

    /// <summary>
    /// Sets the accept.
    /// </summary>
    /// <param name="accept">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminDecideTranslationRevisionRequestBuilder WithAccept(bool accept)
    {
        _accept = accept;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminDecideTranslationRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminDecideTranslationRevisionRequest instance.</returns>
    public AdminDecideTranslationRevisionRequest Build()
    {
        return new AdminDecideTranslationRevisionRequest(Accept: _accept);
    }
}

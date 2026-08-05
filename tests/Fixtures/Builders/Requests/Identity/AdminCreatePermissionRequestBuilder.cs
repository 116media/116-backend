using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreatePermissionRequest"/> instances in tests.
/// </summary>
public class AdminCreatePermissionRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _resource;
    private string _action;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreatePermissionRequestBuilder"/> class
    /// with valid random default values that satisfy the create permission validator.
    /// </summary>
    public AdminCreatePermissionRequestBuilder()
    {
        _resource = BuildShortValue(prefix: "res");
        _action = BuildShortValue(prefix: "act");
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Builds a unique lowercase token that fits within the permission resource/action max length.
    /// </summary>
    /// <param name="prefix">The leading token used to keep generated values readable.</param>
    /// <returns>A unique value bounded by the configured max length.</returns>
    private string BuildShortValue(string prefix)
    {
        string suffix = _faker.Random.AlphaNumeric(length: 8).ToLowerInvariant();
        string candidate = $"{prefix}{suffix}";
        return candidate[..Math.Min(TestConstants.Permission.ResourceMaxLength, candidate.Length)];
    }

    /// <summary>
    /// Sets the permission resource name.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePermissionRequestBuilder WithResource(string resource)
    {
        _resource = resource;
        return this;
    }

    /// <summary>
    /// Sets the permission action name.
    /// </summary>
    /// <param name="action">The action name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePermissionRequestBuilder WithAction(string action)
    {
        _action = action;
        return this;
    }

    /// <summary>
    /// Sets the permission description.
    /// </summary>
    /// <param name="description">The permission description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreatePermissionRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreatePermissionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreatePermissionRequest instance.</returns>
    public AdminCreatePermissionRequest Build()
    {
        return new AdminCreatePermissionRequest(Resource: _resource, Action: _action, Description: _description);
    }
}

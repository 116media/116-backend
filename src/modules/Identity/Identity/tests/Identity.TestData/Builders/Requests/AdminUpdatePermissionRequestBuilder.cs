using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission.V1;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using Bogus;

namespace _116.Identity.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdatePermissionRequest"/> instances in tests.
/// </summary>
public class AdminUpdatePermissionRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string? _resource;
    private string? _action;
    private string? _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdatePermissionRequestBuilder"/> class
    /// with valid random default values that satisfy the update permission validator.
    /// </summary>
    public AdminUpdatePermissionRequestBuilder()
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
    /// Builds the <see cref="AdminUpdatePermissionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdatePermissionRequest instance.</returns>
    public AdminUpdatePermissionRequest Build()
    {
        return new AdminUpdatePermissionRequest(Resource: _resource, Action: _action, Description: _description);
    }
}

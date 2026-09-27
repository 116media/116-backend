using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole.V1;
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
/// Fluent builder for creating <see cref="AdminUpdateRoleRequest"/> instances in tests.
/// </summary>
public class AdminUpdateRoleRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string? _name;
    private string? _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateRoleRequestBuilder"/> class
    /// with valid random default values that satisfy the update role validator.
    /// </summary>
    public AdminUpdateRoleRequestBuilder()
    {
        string suffix = _faker.Random.AlphaNumeric(length: 8);
        string candidate = $"r{suffix}";
        _name = candidate[..Math.Min(TestConstants.Role.NameMaxLength, candidate.Length)];
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateRoleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateRoleRequest instance.</returns>
    public AdminUpdateRoleRequest Build()
    {
        return new AdminUpdateRoleRequest(Name: _name, Description: _description);
    }
}

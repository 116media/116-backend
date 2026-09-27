using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole.V1;
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
/// Fluent builder for creating <see cref="AdminCreateRoleRequest"/> instances in tests.
/// </summary>
public class AdminCreateRoleRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;
    private string _description;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateRoleRequestBuilder"/> class
    /// with valid random default values that satisfy the create role validator.
    /// </summary>
    public AdminCreateRoleRequestBuilder()
    {
        string suffix = _faker.Random.AlphaNumeric(length: 8);
        string candidate = $"r{suffix}";
        _name = candidate[..Math.Min(TestConstants.Role.NameMaxLength, candidate.Length)];
        _description = _faker.Lorem.Sentence(wordCount: 5);
    }

    /// <summary>
    /// Sets the role name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateRoleRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateRoleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateRoleRequest instance.</returns>
    public AdminCreateRoleRequest Build()
    {
        return new AdminCreateRoleRequest(Name: _name, Description: _description);
    }
}

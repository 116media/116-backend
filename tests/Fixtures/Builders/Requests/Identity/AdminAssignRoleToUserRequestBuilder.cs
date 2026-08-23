using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminAssignRoleToUserRequest"/> instances in tests.
/// </summary>
public class AdminAssignRoleToUserRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _roleId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAssignRoleToUserRequestBuilder"/> class
    /// with a valid random role identifier that satisfies the validator.
    /// </summary>
    public AdminAssignRoleToUserRequestBuilder()
    {
        _roleId = _faker.Random.Guid();
    }

    /// <summary>
    /// Sets the identifier of the role to assign to the user.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAssignRoleToUserRequestBuilder WithRoleId(Guid roleId)
    {
        _roleId = roleId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAssignRoleToUserRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAssignRoleToUserRequest instance.</returns>
    public AdminAssignRoleToUserRequest Build()
    {
        return new AdminAssignRoleToUserRequest(RoleId: _roleId);
    }
}

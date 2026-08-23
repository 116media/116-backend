using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminAssignPermissionToRoleRequest"/> instances in tests.
/// </summary>
public class AdminAssignPermissionToRoleRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _permissionId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAssignPermissionToRoleRequestBuilder"/> class
    /// with a valid random permission identifier.
    /// </summary>
    public AdminAssignPermissionToRoleRequestBuilder()
    {
        _permissionId = _faker.Random.Guid();
    }

    /// <summary>
    /// Sets the identifier of the permission to assign to the role.
    /// </summary>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAssignPermissionToRoleRequestBuilder WithPermissionId(Guid permissionId)
    {
        _permissionId = permissionId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAssignPermissionToRoleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAssignPermissionToRoleRequest instance.</returns>
    public AdminAssignPermissionToRoleRequest Build()
    {
        return new AdminAssignPermissionToRoleRequest(PermissionId: _permissionId);
    }
}

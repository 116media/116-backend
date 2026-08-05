using _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Identity;

/// <summary>
/// Fluent builder for creating <see cref="AdminBulkUpdateRolePermissionsRequest"/> instances in tests.
/// </summary>
public class AdminBulkUpdateRolePermissionsRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private List<Guid> _permissionIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminBulkUpdateRolePermissionsRequestBuilder"/> class
    /// with a valid random list of permission identifiers.
    /// </summary>
    public AdminBulkUpdateRolePermissionsRequestBuilder()
    {
        _permissionIds = [_faker.Random.Guid(), _faker.Random.Guid()];
    }

    /// <summary>
    /// Sets the list of permission identifiers to assign to the role.
    /// </summary>
    /// <param name="permissionIds">The permission identifiers.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminBulkUpdateRolePermissionsRequestBuilder WithPermissionIds(List<Guid> permissionIds)
    {
        _permissionIds = permissionIds;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminBulkUpdateRolePermissionsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminBulkUpdateRolePermissionsRequest instance.</returns>
    public AdminBulkUpdateRolePermissionsRequest Build()
    {
        return new AdminBulkUpdateRolePermissionsRequest(PermissionIds: _permissionIds);
    }
}

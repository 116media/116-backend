using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles;
using _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.MetaFields;

/// <summary>
/// Unit tests for GetOwnRoles MetaField static initializers.
/// Accessing each static readonly field triggers the class initializer, achieving 100% line coverage.
/// </summary>
public class GetOwnRolesMetaFieldTests
{
    [Fact]
    public void AdminGetOwnRolesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetOwnRolesMetaField.GetOwnRoles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetOwnRolesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetOwnRolesMetaField.GetOwnRoles;
        metadata.Should().NotBeNull();
    }
}

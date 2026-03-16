using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;

/// <summary>
/// Unit tests for <see cref="AdminScheduleShootResponse"/>.
/// </summary>
public class AdminScheduleShootEndpointV1Tests
{
    [Fact]
    public void AdminScheduleShootResponse_ShouldConstructCorrectly()
    {
        // Act
        var response = new AdminScheduleShootResponse(IsSuccess: true);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}

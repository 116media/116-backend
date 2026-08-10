using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="CustomerEntity"/>.
/// </summary>
public class CustomerEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreateCustomer()
    {
        // Arrange
        var id = Guid.NewGuid();
        string fullName = TestConstants.Customer.ValidFullName;
        string email = TestConstants.Customer.ValidEmail;

        // Act
        var entity = CustomerEntity.Create(
            id,
            fullName,
            email,
            null,
            null,
            null,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Assert
        entity.Id.Should().Be(id);
        entity.FullName.Should().Be(fullName);
        entity.Email.Should().Be(email);
        entity.Phone.Should().BeNull();
        entity.Company.Should().BeNull();
        entity.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllOptionalFields_ShouldSetAllFields()
    {
        // Act
        var entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Customer.ValidFullName,
            TestConstants.Customer.ValidEmail,
            TestConstants.Customer.ValidPhone,
            TestConstants.Customer.ValidCompany,
            TestConstants.Customer.ValidNotes,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Assert
        entity.Phone.Should().Be(TestConstants.Customer.ValidPhone);
        entity.Company.Should().Be(TestConstants.Customer.ValidCompany);
        entity.Notes.Should().Be(TestConstants.Customer.ValidNotes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFullName_ShouldThrowBadRequestException(string? invalidFullName)
    {
        // Act
        Action act = () =>
            CustomerEntity.Create(
                Guid.NewGuid(),
                invalidFullName!,
                TestConstants.Customer.ValidEmail,
                null,
                null,
                null,
                TestErrorsFactory.CreateCustomerErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrowBadRequestException(string? invalidEmail)
    {
        // Act
        Action act = () =>
            CustomerEntity.Create(
                Guid.NewGuid(),
                TestConstants.Customer.ValidFullName,
                invalidEmail!,
                null,
                null,
                null,
                TestErrorsFactory.CreateCustomerErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidValues_ShouldUpdateFields()
    {
        // Arrange
        var entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Customer.ValidFullName,
            TestConstants.Customer.ValidEmail,
            null,
            null,
            null,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Act
        entity.Update(
            "New Name",
            "new@example.com",
            TestConstants.Customer.ValidPhone,
            "New Company",
            "Some notes",
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Assert
        entity.FullName.Should().Be("New Name");
        entity.Email.Should().Be("new@example.com");
        entity.Phone.Should().Be(TestConstants.Customer.ValidPhone);
        entity.Company.Should().Be("New Company");
        entity.Notes.Should().Be("Some notes");
    }

    [Fact]
    public void Update_ShouldChangeEmail()
    {
        // Arrange
        var entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Customer.ValidFullName,
            TestConstants.Customer.ValidEmail,
            null,
            null,
            null,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Act
        entity.Update("New Name", "updated@example.com", null, null, null, TestErrorsFactory.CreateCustomerErrors());

        // Assert
        entity.Email.Should().Be("updated@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidFullName_ShouldThrowBadRequestException(string? invalidFullName)
    {
        // Arrange
        var entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Customer.ValidFullName,
            TestConstants.Customer.ValidEmail,
            null,
            null,
            null,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Act
        Action act = () =>
            entity.Update(
                invalidFullName!,
                TestConstants.Customer.ValidEmail,
                null,
                null,
                null,
                TestErrorsFactory.CreateCustomerErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidEmail_ShouldThrowBadRequestException(string? invalidEmail)
    {
        // Arrange
        var entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Customer.ValidFullName,
            TestConstants.Customer.ValidEmail,
            null,
            null,
            null,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Act
        Action act = () =>
            entity.Update(
                TestConstants.Customer.ValidFullName,
                invalidEmail!,
                null,
                null,
                null,
                TestErrorsFactory.CreateCustomerErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Update_WithNullOptionalFields_ShouldClearThem()
    {
        // Arrange
        var entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Customer.ValidFullName,
            TestConstants.Customer.ValidEmail,
            TestConstants.Customer.ValidPhone,
            TestConstants.Customer.ValidCompany,
            TestConstants.Customer.ValidNotes,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Act
        entity.Update(
            "Updated Name",
            "updated@example.com",
            null,
            null,
            null,
            TestErrorsFactory.CreateCustomerErrors()
        );

        // Assert
        entity.FullName.Should().Be("Updated Name");
        entity.Email.Should().Be("updated@example.com");
        entity.Phone.Should().BeNull();
        entity.Company.Should().BeNull();
        entity.Notes.Should().BeNull();
    }

    #endregion
}

using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
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
        Guid id = Guid.NewGuid();
        string fullName = TestConstants.Content.Customer.ValidFullName;
        string email = TestConstants.Content.Customer.ValidEmail;

        // Act
        CustomerEntity entity = CustomerEntity.Create(id, fullName, email, null, null, null);

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
        CustomerEntity entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Customer.ValidFullName,
            TestConstants.Content.Customer.ValidEmail,
            TestConstants.Content.Customer.ValidPhone,
            TestConstants.Content.Customer.ValidCompany,
            TestConstants.Content.Customer.ValidNotes
        );

        // Assert
        entity.Phone.Should().Be(TestConstants.Content.Customer.ValidPhone);
        entity.Company.Should().Be(TestConstants.Content.Customer.ValidCompany);
        entity.Notes.Should().Be(TestConstants.Content.Customer.ValidNotes);
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
                TestConstants.Content.Customer.ValidEmail,
                null,
                null,
                null
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
                TestConstants.Content.Customer.ValidFullName,
                invalidEmail!,
                null,
                null,
                null
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
        CustomerEntity entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Customer.ValidFullName,
            TestConstants.Content.Customer.ValidEmail,
            null,
            null,
            null
        );

        // Act
        entity.Update("New Name", TestConstants.Content.Customer.ValidPhone, "New Company", "Some notes");

        // Assert
        entity.FullName.Should().Be("New Name");
        entity.Phone.Should().Be(TestConstants.Content.Customer.ValidPhone);
        entity.Company.Should().Be("New Company");
        entity.Notes.Should().Be("Some notes");
    }

    [Fact]
    public void Update_ShouldNotChangeEmail()
    {
        // Arrange
        CustomerEntity entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Customer.ValidFullName,
            TestConstants.Content.Customer.ValidEmail,
            null,
            null,
            null
        );
        string originalEmail = entity.Email;

        // Act
        entity.Update("New Name", null, null, null);

        // Assert
        entity.Email.Should().Be(originalEmail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidFullName_ShouldThrowBadRequestException(string? invalidFullName)
    {
        // Arrange
        CustomerEntity entity = CustomerEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Customer.ValidFullName,
            TestConstants.Content.Customer.ValidEmail,
            null,
            null,
            null
        );

        // Act
        Action act = () => entity.Update(invalidFullName!, null, null, null);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}

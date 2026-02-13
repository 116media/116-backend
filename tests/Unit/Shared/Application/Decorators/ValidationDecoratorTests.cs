using _116.Shared.Application.Decorators;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Decorators;

/// <summary>
/// Unit tests for <see cref="ValidationDecorator{TRequest, TResponse}"/>.
/// </summary>
public class ValidationDecoratorTests
{
    #region Test Setup

    public record TestRequest(string Value) : IRequest<TestResponse>;

    public record TestResponse(string Result);

    private class TestValidator : AbstractValidator<TestRequest>
    {
        public TestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    private class AlwaysFailsValidator : AbstractValidator<TestRequest>
    {
        public AlwaysFailsValidator()
        {
            RuleFor(x => x.Value).Must(_ => false).WithMessage("Always fails");
        }
    }

    private class CustomValidator : AbstractValidator<TestRequest>
    {
        public CustomValidator(string errorMessage)
        {
            RuleFor(x => x.Value).Must(_ => false).WithMessage(errorMessage);
        }
    }

    #endregion

    #region Success Cases - No Validators

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallHandler()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        IEnumerable<IValidator<TestRequest>> validators = [];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        TestRequest request = new("test");

        // Act
        TestResponse response = await decorator.Handle(request);

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().Be("Success");
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldReturnHandlerResult()
    {
        // Arrange
        TestResponse expectedResponse = new("Expected result");
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        IEnumerable<IValidator<TestRequest>> validators = [];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        // Act
        TestResponse response = await decorator.Handle(new TestRequest("test"));

        // Assert
        response.Should().Be(expectedResponse);
    }

    #endregion

    #region Success Cases - Valid Request

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallHandler()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        IEnumerable<IValidator<TestRequest>> validators = [new TestValidator()];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        TestRequest request = new("valid value");

        // Act
        TestResponse response = await decorator.Handle(request);

        // Assert
        response.Result.Should().Be("Success");
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequestAndMultipleValidators_ShouldPassAllValidations()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        IEnumerable<IValidator<TestRequest>> validators =
        [
            new TestValidator(),
            new TestValidator(), // Same validator twice to ensure both run
        ];

        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        // Act
        TestResponse response = await decorator.Handle(new TestRequest("valid"));

        // Assert
        response.Should().NotBeNull();
        handlerMock.Verify(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Failure Cases - Invalid Request

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();

        IEnumerable<IValidator<TestRequest>> validators = [new TestValidator()];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        TestRequest request = new(""); // Empty value should fail validation

        // Act
        Func<Task> act = async () => await decorator.Handle(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldNotCallHandler()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();

        IEnumerable<IValidator<TestRequest>> validators = [new TestValidator()];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        TestRequest request = new("");

        // Act
        try
        {
            await decorator.Handle(request);
        }
        catch (ValidationException)
        {
            // Expected
        }

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleValidationFailures_ShouldThrowValidationExceptionWithAllErrors()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();

        IEnumerable<IValidator<TestRequest>> validators =
        [
            new CustomValidator("Error 1"),
            new CustomValidator("Error 2"),
            new CustomValidator("Error 3"),
        ];

        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        // Act
        Func<Task> act = async () => await decorator.Handle(new TestRequest("test"));

        // Assert
        ExceptionAssertions<ValidationException> exception = await act.Should()
            .ThrowExactlyAsync<ValidationException>();
        exception.Which.Errors.Should().NotBeEmpty();
        exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error 1");
        exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error 2");
        exception.Which.Errors.Should().Contain(e => e.ErrorMessage == "Error 3");
    }

    [Fact]
    public async Task Handle_WithValidationFailure_ShouldIncludeValidationErrors()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();

        IEnumerable<IValidator<TestRequest>> validators = [new AlwaysFailsValidator()];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        // Act
        Func<Task> act = async () => await decorator.Handle(new TestRequest("test"));

        // Assert
        ExceptionAssertions<ValidationException> exception = await act.Should()
            .ThrowExactlyAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(1);
        exception.Which.Errors.First().ErrorMessage.Should().Be("Always fails");
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToValidator()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<IValidator<TestRequest>> validatorMock = new();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        IEnumerable<IValidator<TestRequest>> validators = [validatorMock.Object];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        // Act
        await decorator.Handle(new TestRequest("test"), token);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToHandler()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        IEnumerable<IValidator<TestRequest>> validators = [new TestValidator()];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        // Act
        await decorator.Handle(new TestRequest("valid"), token);

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<TestRequest>(), token), Times.Once);
    }

    #endregion

    #region Validator Execution Tests

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAllValidators()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<IValidator<TestRequest>> validator1Mock = new();
        Mock<IValidator<TestRequest>> validator2Mock = new();
        Mock<IValidator<TestRequest>> validator3Mock = new();

        validator1Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validator2Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validator3Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        IEnumerable<IValidator<TestRequest>> validators =
        [
            validator1Mock.Object,
            validator2Mock.Object,
            validator3Mock.Object,
        ];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        validator1Mock.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        validator2Mock.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        validator3Mock.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithMixedValidationResults_ShouldCollectOnlyFailures()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();

        Mock<IValidator<TestRequest>> passingValidator = new();
        passingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // No errors

        Mock<IValidator<TestRequest>> failingValidator = new();
        failingValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ValidationResult(
                    new List<ValidationFailure> { new ValidationFailure("Value", "Validation failed") }
                )
            );

        IEnumerable<IValidator<TestRequest>> validators = [passingValidator.Object, failingValidator.Object];
        ValidationDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, validators);

        // Act
        Func<Task> act = async () => await decorator.Handle(new TestRequest("test"));

        // Assert
        ExceptionAssertions<ValidationException> exception = await act.Should()
            .ThrowExactlyAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle();
        exception.Which.Errors.First().ErrorMessage.Should().Be("Validation failed");
    }

    #endregion
}

using _116.Shared.Contracts.Application.CQRS;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Shared.Application.Decorators;

/// <summary>
/// Decorator that validates requests using FluentValidation before executing the handler.
/// Throws ValidationException if any validation rules fail.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class ValidationDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    IEnumerable<IValidator<TRequest>> validators
) : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request if validators are present
        if (!validators.Any())
        {
            return await handler.Handle(request, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        List<ValidationFailure> failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await handler.Handle(request, cancellationToken);
    }
}

/// <summary>
/// Decorator that validates void requests using FluentValidation before executing the handler.
/// Throws ValidationException if any validation rules fail.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public class ValidationDecorator<TRequest>(
    IRequestHandler<TRequest> handler,
    IEnumerable<IValidator<TRequest>> validators
) : IRequestHandler<TRequest> where TRequest : IRequest
{
    public async Task Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request if validators are present
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            ValidationResult[] validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            List<ValidationFailure> failures = validationResults
                .Where(r => r.Errors.Count != 0)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        await handler.Handle(request, cancellationToken);
    }
}

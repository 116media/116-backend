using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Customer domain error factory providing simple, readable exception creation.
/// Usage: CustomerErrors.AlreadyExists(email) or CustomerErrors.NotFound(id)
/// </summary>
public static class CustomerErrors
{
    /// <summary>Throws when a customer with the given email already exists.</summary>
    public static ConflictException AlreadyExists(string email)
    {
        return new ConflictException(CustomerErrorMessage.AlreadyExists(email: email));
    }

    /// <summary>Throws when a customer is not found by its identifier.</summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Customer", "id", keyValue: id);
    }

    /// <summary>Throws when a customer full name is required but not provided.</summary>
    public static BadRequestException FullNameRequired()
    {
        return new BadRequestException(CustomerErrorMessage.FullNameRequired());
    }

    /// <summary>Throws when a customer email is required but not provided.</summary>
    public static BadRequestException EmailRequired()
    {
        return new BadRequestException(CustomerErrorMessage.EmailRequired());
    }
}

using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Customer domain error factory providing simple, readable exception creation.
/// Usage: CustomerErrors.AlreadyExists(email) or CustomerErrors.NotFound(id)
/// </summary>
public class CustomerErrors(CustomerErrorMessage i18n)
{
    /// <summary>
    /// Throws when a customer with the given email already exists.
    /// </summary>
    public ConflictException AlreadyExists(string email)
    {
        return new ConflictException(i18n.AlreadyExists(email: email));
    }

    /// <summary>
    /// Throws when a customer is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Customer", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a customer full name is required but not provided.
    /// </summary>
    public BadRequestException FullNameRequired()
    {
        return new BadRequestException(i18n.FullNameRequired());
    }

    /// <summary>
    /// Throws when a customer email is required but not provided.
    /// </summary>
    public BadRequestException EmailRequired()
    {
        return new BadRequestException(i18n.EmailRequired());
    }
}

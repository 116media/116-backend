using System.Linq.Expressions;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Auth.Specifications;

/// <summary>
/// Specification that matches users by their email address.
/// Performs exact string comparison for email lookup operations.
/// </summary>
public class UserByEmailSpecification(string email) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        // Parsed once outside the expression so the query compares value objects directly.
        var target = new Email(value: email);
        return user => user.Email == target;
    }
}

/// <summary>
/// Specification that matches users by their username.
/// Performs exact string comparison for username lookup operations.
/// </summary>
public class UserByUserNameSpecification(string userName) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.UserName == userName;
    }
}

/// <summary>
/// Specification that matches users by their full phone number.
/// Useful for phone number uniqueness validation and user lookup by phone.
/// </summary>
public class UserByPhoneNumberSpecification(string phoneNumber) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.FullPhoneNumber == phoneNumber;
    }
}

/// <summary>
/// Specification that matches users by their unique identifier.
/// Used for direct user lookup operations.
/// </summary>
public class UserByIdSpecification(Guid userId) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.Id == userId;
    }
}

/// <summary>
/// Specification that matches users by login credentials (email or username).
/// Automatically detects whether the provided credentials are an email or username
/// based on the presence of '@' and '.' characters.
/// </summary>
public class UserByCredentialsSpecification(string credentials) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        // A credential that does not parse as an address can only be a username; parsing
        // outside the expression keeps the throwing conversion out of query translation.
        Email? asEmail = Email.TryFrom(value: credentials);
        return asEmail != null ? user => user.Email == asEmail : user => user.UserName == credentials;
    }
}

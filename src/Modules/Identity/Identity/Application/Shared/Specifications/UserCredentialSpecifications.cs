using _116.Shared.Application.Specifications;
using _116.Identity.Domain.Entities;
using System.Linq.Expressions;

namespace _116.Identity.Application.Shared.Specifications;

/// <summary>
/// Specification that matches users by their email address.
/// Performs exact string comparison for email lookup operations.
/// </summary>
public class UserByEmailSpecification(string email) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.Email == email;
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
        bool isEmail = credentials.Contains('@') && credentials.Contains('.');
        return isEmail
            ? user => user.Email == credentials
            : user => user.UserName == credentials;
    }
}

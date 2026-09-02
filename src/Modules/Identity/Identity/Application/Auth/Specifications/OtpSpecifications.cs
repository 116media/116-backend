using System.Linq.Expressions;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Auth.Specifications;

/// <summary>
/// Specification that matches OTPs by user ID.
/// Used for filtering OTPs belonging to a specific user.
/// </summary>
public class OtpByUserIdSpecification(Guid userId) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.UserId == userId;
    }
}

/// <summary>
/// Specification that matches OTPs by purpose.
/// Used for filtering OTPs based on their intended purpose.
/// </summary>
public class OtpByPurposeSpecification(EnumOtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.Purpose == purpose;
    }
}

/// <summary>
/// Specification that matches unused OTPs.
/// Used for filtering OTPs that haven't been used yet.
/// </summary>
public class OtpIsNotUsedSpecification : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.IsUsed == false;
    }
}

/// <summary>
/// Specification that matches used OTPs.
/// Used for filtering OTPs that have been marked as used.
/// </summary>
public class OtpIsUsedSpecification : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.IsUsed == true;
    }
}

/// <summary>
/// Specification that matches expired OTPs.
/// Used for cleanup operations and filtering expired OTPs.
/// </summary>
public class OtpIsExpiredSpecification : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.ExpiresAt <= DateTime.UtcNow;
    }
}

/// <summary>
/// Composite specification that matches the OTPs a verification attempt may be checked against.
/// Combines user ID, purpose, and not used specifications.
/// The supplied code is compared against the stored hash in memory, never in the query.
/// </summary>
public class OtpForValidationSpecification(Guid userId, EnumOtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        var userSpec = new OtpByUserIdSpecification(userId: userId);
        var purposeSpec = new OtpByPurposeSpecification(purpose: purpose);
        var notUsedSpec = new OtpIsNotUsedSpecification();
        var notConsumedSpec = new OtpIsNotConsumedSpecification();
        return userSpec.And(other: purposeSpec).And(other: notUsedSpec).And(other: notConsumedSpec).ToExpression();
    }
}

/// <summary>
/// Composite specification that matches unused OTPs for a user and purpose.
/// Combines user ID, purpose, and not used specifications.
/// Used for invalidating existing OTPs when generating new ones.
/// </summary>
public class OtpForInvalidationSpecification(Guid userId, EnumOtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        var userSpec = new OtpByUserIdSpecification(userId: userId);
        var purposeSpec = new OtpByPurposeSpecification(purpose: purpose);
        var notUsedSpec = new OtpIsNotUsedSpecification();
        var notConsumedSpec = new OtpIsNotConsumedSpecification();
        return userSpec.And(other: purposeSpec).And(other: notUsedSpec).And(other: notConsumedSpec).ToExpression();
    }
}

/// <summary>
/// Specification matching OTPs that have not been spent or superseded.
/// </summary>
public class OtpIsNotConsumedSpecification : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.ConsumedAt == null;
    }
}

/// <summary>
/// Composite specification that matches the used OTPs a re-validation attempt may be checked against.
/// Combines user ID, purpose, and used specifications.
/// The supplied code is compared against the stored hash in memory, never in the query.
/// </summary>
public class OtpForUsedValidationSpecification(Guid userId, EnumOtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        var userSpec = new OtpByUserIdSpecification(userId: userId);
        var purposeSpec = new OtpByPurposeSpecification(purpose: purpose);
        var usedSpec = new OtpIsUsedSpecification();
        var notConsumedSpec = new OtpIsNotConsumedSpecification();
        return userSpec.And(other: purposeSpec).And(other: usedSpec).And(other: notConsumedSpec).ToExpression();
    }
}

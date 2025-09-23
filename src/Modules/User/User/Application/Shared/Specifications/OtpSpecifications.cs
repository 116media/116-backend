using _116.Shared.Application.Specifications;
using _116.User.Domain.Entities;
using _116.User.Domain.Enums;
using System.Linq.Expressions;

namespace _116.User.Application.Shared.Specifications;

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
public class OtpByPurposeSpecification(OtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.Purpose == purpose;
    }
}

/// <summary>
/// Specification that matches OTPs by code.
/// Used for finding OTPs with a specific verification code.
/// </summary>
public class OtpByCodeSpecification(string code) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.Code == code;
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
/// Specification that matches non-expired OTPs.
/// Used for filtering OTPs that are still valid based on expiration time.
/// </summary>
public class OtpIsNotExpiredSpecification : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        return otp => otp.ExpiresAt > DateTime.UtcNow;
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
/// Composite specification that matches valid OTPs for a user and purpose.
/// Combines user ID, purpose, not used, and not expired specifications.
/// Used for finding OTPs that can be validated.
/// </summary>
public class OtpIsValidForUserAndPurposeSpecification(Guid userId, OtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        var userSpec = new OtpByUserIdSpecification(userId);
        var purposeSpec = new OtpByPurposeSpecification(purpose);
        var notUsedSpec = new OtpIsNotUsedSpecification();
        var notExpiredSpec = new OtpIsNotExpiredSpecification();

        return otp => userSpec.IsSatisfiedBy(otp) &&
                     purposeSpec.IsSatisfiedBy(otp) &&
                     notUsedSpec.IsSatisfiedBy(otp) &&
                     notExpiredSpec.IsSatisfiedBy(otp);
    }
}

/// <summary>
/// Composite specification that matches OTPs for validation.
/// Combines user ID, code, purpose, and not used specifications.
/// Used for finding OTPs during the validation process.
/// </summary>
public class OtpForValidationSpecification(Guid userId, string code, OtpPurpose purpose) : Specification<OtpEntity>
{
    public override Expression<Func<OtpEntity, bool>> ToExpression()
    {
        var userSpec = new OtpByUserIdSpecification(userId);
        var codeSpec = new OtpByCodeSpecification(code);
        var purposeSpec = new OtpByPurposeSpecification(purpose);
        var notUsedSpec = new OtpIsNotUsedSpecification();

        return otp => userSpec.IsSatisfiedBy(otp) &&
                     codeSpec.IsSatisfiedBy(otp) &&
                     purposeSpec.IsSatisfiedBy(otp) &&
                     notUsedSpec.IsSatisfiedBy(otp);
    }
}

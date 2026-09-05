using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ContentPaymentEntity"/> domain behaviour.
/// </summary>
public class ContentPaymentEntityTests
{
    private readonly ContentOrderErrors _errors = TestErrorsFactory.CreateContentOrderErrors();

    #region Create

    [Fact]
    public void Create_ShouldSetId_OrderId_AmountUsd_StatusPending()
    {
        Guid id = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        const decimal amount = 150m;

        ContentPaymentEntity payment = ContentPaymentEntity.Create(id, orderId, amount);

        payment.Id.Should().Be(id);
        payment.OrderId.Should().Be(orderId);
        payment.AmountUsd.Should().Be(amount);
        payment.Status.Should().Be(EnumPaymentStatus.Pending);
        payment.PaymentProofFileId.Should().BeNull();
    }

    #endregion

    #region AttachProof

    [Fact]
    public void AttachProof_ShouldSetProofFileIdAndPaymentMethod()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        Guid proofFileId = Guid.NewGuid();

        payment.AttachProof(proofFileId, EnumPaymentMethod.BankTransfer);

        payment.PaymentProofFileId.Should().Be(proofFileId);
        payment.PaymentMethod.Should().Be(EnumPaymentMethod.BankTransfer);
    }

    #endregion

    #region Verify

    [Fact]
    public void Verify_WithoutProofAttached_ShouldThrowPaymentProofRequired()
    {
        // Arrange — an unevidenced payment must never verify and flip the order to paid
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();

        // Act
        Action act = () => payment.Verify(Guid.NewGuid(), "https://receipts.example.com/r.pdf");

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentProofRequired);
        payment.Status.Should().Be(EnumPaymentStatus.Pending);
    }

    [Fact]
    public void AttachProof_WhenPending_ShouldRecordProofAndMethod()
    {
        // Arrange
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        Guid proofFileId = Guid.NewGuid();

        // Act
        payment.AttachProof(proofFileId, EnumPaymentMethod.BankTransfer);

        // Assert
        payment.PaymentProofFileId.Should().Be(proofFileId);
        payment.PaymentMethod.Should().Be(EnumPaymentMethod.BankTransfer);
    }

    [Fact]
    public void AttachProof_WhenVerified_ShouldThrowAndKeepTheOriginalProof()
    {
        // Arrange — overwriting proof on a decided payment would destroy the evidence the
        // verification was based on
        Guid originalProofId = Guid.NewGuid();
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        payment.AttachProof(originalProofId, EnumPaymentMethod.BankTransfer);
        payment.Verify(Guid.NewGuid(), "https://receipts.example.com/r.pdf");

        // Act
        Action act = () => payment.AttachProof(Guid.NewGuid(), EnumPaymentMethod.MobileMoney);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentAlreadyDecided);
        payment.PaymentProofFileId.Should().Be(originalProofId);
    }

    [Fact]
    public void AttachProof_WhenRejected_ShouldThrowConflict()
    {
        // Arrange
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        payment.Reject(notes: null);

        // Act
        Action act = () => payment.AttachProof(Guid.NewGuid(), EnumPaymentMethod.BankTransfer);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentAlreadyDecided);
    }

    [Fact]
    public void Verify_WhenPending_ShouldTransitionToVerified_AndSetFields()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        payment.AttachProof(Guid.NewGuid(), EnumPaymentMethod.BankTransfer);
        Guid adminId = Guid.NewGuid();
        const string receiptUrl = "https://receipts.example.com/receipt-123.pdf";

        payment.Verify(adminId, receiptUrl);

        payment.Status.Should().Be(EnumPaymentStatus.Verified);
        payment.VerifiedById.Should().Be(adminId);
        payment.ReceiptUrl.Should().Be(receiptUrl);
        payment.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Verify_WhenAlreadyVerified_ShouldThrowConflictException()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(Guid.NewGuid());

        Action act = () => payment.Verify(Guid.NewGuid(), "https://receipts.example.com/receipt.pdf");

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentAlreadyVerified);
    }

    [Fact]
    public void Verify_WhenAlreadyRejected_ShouldThrowConflictException()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateRejected(Guid.NewGuid());

        Action act = () => payment.Verify(Guid.NewGuid(), "https://receipts.example.com/receipt.pdf");

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentAlreadyRejected);
    }

    #endregion

    #region Reject

    [Fact]
    public void Reject_WhenPending_ShouldTransitionToRejected_AndSetNotes()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        const string notes = "Proof is not legible.";

        payment.Reject(notes);

        payment.Status.Should().Be(EnumPaymentStatus.Rejected);
        payment.Notes.Should().Be(notes);
    }

    [Fact]
    public void Reject_WhenPending_ShouldRaisePaymentRejectedEvent()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();
        const string notes = "Proof is not legible.";

        payment.Reject(notes);

        payment
            .DomainEvents.OfType<PaymentRejectedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new PaymentRejectedEvent(payment.OrderId, payment.Id, notes));
    }

    [Fact]
    public void Reject_WithNullNotes_ShouldRaisePaymentRejectedEventWithNullNotes()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateDefault();

        payment.Reject(null);

        payment.DomainEvents.OfType<PaymentRejectedEvent>().Should().ContainSingle().Which.Notes.Should().BeNull();
    }

    [Fact]
    public void Reject_WhenAlreadyVerified_ShouldThrowConflictExceptionAndRaiseNothing()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateVerified(Guid.NewGuid());
        payment.ClearDomainEvents();

        Action act = () => payment.Reject("notes");

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentAlreadyVerified);
        payment.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldThrowConflictException()
    {
        ContentPaymentEntity payment = ContentPaymentFactory.CreateRejected(Guid.NewGuid());

        Action act = () => payment.Reject("notes");

        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PaymentAlreadyRejected);
    }

    #endregion
}

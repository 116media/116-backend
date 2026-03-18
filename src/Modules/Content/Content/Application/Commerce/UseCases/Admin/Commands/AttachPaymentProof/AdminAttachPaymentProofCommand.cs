using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Command for uploading a payment proof file and attaching it to an order's payment record.
/// Accepts images (JPEG, PNG, GIF, WebP) or PDF documents up to 5 MB.
/// </summary>
/// <param name="OrderId">The identifier of the order.</param>
/// <param name="File">The payment proof file to upload to Cloudinary.</param>
/// <param name="PaymentMethod">The payment method used.</param>
public record AdminAttachPaymentProofCommand(string OrderId, IFormFile File, EnumPaymentMethod PaymentMethod)
    : ICommand<AdminAttachPaymentProofResult>;

/// <summary>
/// Result returned after successfully attaching a payment proof.
/// </summary>
/// <param name="Proof">Metadata of the uploaded proof file, including MIME type for frontend rendering.</param>
public record AdminAttachPaymentProofResult(FileDto Proof);

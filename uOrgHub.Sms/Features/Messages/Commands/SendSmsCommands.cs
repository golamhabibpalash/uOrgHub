using MediatR;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Messages.Commands;

/// <param name="AllowBulk">Whether the caller holds the bulk-send permission (checked by the controller).</param>
public record SendSmsCommand(SendSmsDto Dto, Guid? UserId, bool AllowBulk) : ICommand<SendSmsResultDto>;

public class SendSmsCommandHandler(SmsSendService sendService) : IRequestHandler<SendSmsCommand, SendSmsResultDto>
{
    public async Task<SendSmsResultDto> Handle(SendSmsCommand request, CancellationToken ct)
    {
        // Counted after normalising, so "01711…, +88 01711…" is one recipient, not a bulk send.
        if (!request.AllowBulk)
        {
            var (valid, _) = SmsPhoneNumber.Parse(request.Dto.Recipients);
            if (valid.Count > 1)
                throw new uOrgHub.Shared.Exceptions.AppException(
                    "Sending to more than one recipient needs the 'Send bulk SMS' permission.", 403);
        }

        var (sendRequest, duplicate) = await sendService.CreateAsync(
            request.Dto.Recipients, request.Dto.Message, request.Dto.IdempotencyKey, "Manual", request.UserId, ct: ct);

        return new SendSmsResultDto
        {
            RequestId = sendRequest.Id,
            Duplicate = duplicate,
            GatewayName = sendRequest.Gateway?.Name ?? string.Empty,
            RecipientCount = sendRequest.RecipientCount,
            SegmentsPerRecipient = sendRequest.SegmentsPerRecipient,
            TotalSegments = sendRequest.TotalSegments,
            Encoding = sendRequest.Encoding,
            EstimatedCost = sendRequest.EstimatedCost,
            Currency = sendRequest.Gateway?.Currency ?? "BDT",
        };
    }
}

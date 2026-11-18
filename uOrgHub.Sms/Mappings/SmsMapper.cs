using Riok.Mapperly.Abstractions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Models.Entities;

namespace uOrgHub.Sms.Mappings;

/// <summary>
/// Entity → DTO copies. Fields computed from the provider descriptor or the stored settings
/// (provider name, capabilities, credential presence) are filled in by <c>SmsGatewayRuntime.ToDto</c>.
/// Credentials have no target here on purpose: the cipher can never reach a DTO by mapping.
/// </summary>
// Only target members must be mapped: entities carry audit/company columns that DTOs leave out on purpose.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SmsMapper
{
    [MapperIgnoreTarget(nameof(SmsGatewayResponseDto.ProviderName))]
    [MapperIgnoreTarget(nameof(SmsGatewayResponseDto.EffectiveBaseUrl))]
    [MapperIgnoreTarget(nameof(SmsGatewayResponseDto.Settings))]
    [MapperIgnoreTarget(nameof(SmsGatewayResponseDto.Credentials))]
    [MapperIgnoreTarget(nameof(SmsGatewayResponseDto.Capabilities))]
    public partial SmsGatewayResponseDto ToDto(SmsGateway gateway);

    [MapperIgnoreTarget(nameof(SmsMessageListItemDto.MessagePreview))]
    public partial SmsMessageListItemDto ToListItem(SmsMessage message);

    [MapperIgnoreTarget(nameof(SmsMessageDetailDto.MessagePreview))]
    [MapProperty(nameof(@SmsMessage.SendRequest.Source), nameof(SmsMessageDetailDto.Source))]
    [MapProperty(nameof(@SmsMessage.SendRequest.SourceReference), nameof(SmsMessageDetailDto.SourceReference))]
    public partial SmsMessageDetailDto ToDetail(SmsMessage message);

    public static string Preview(string text) => text.Length <= 80 ? text : text[..80] + "…";
}

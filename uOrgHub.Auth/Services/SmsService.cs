using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace uOrgHub.Auth.Services;

public class SmsService : ISmsService
{
    private readonly IConfiguration _config;

    public SmsService(IConfiguration config) => _config = config;

    public async Task SendOtpAsync(string phoneNumber, string code, string purpose)
    {
        TwilioClient.Init(
            _config["SmsSettings:AccountSid"]!,
            _config["SmsSettings:AuthToken"]!);

        var text = purpose switch
        {
            "Login" => $"uOrgHub 2FA code: {code}. Expires in 10 minutes.",
            "PasswordReset" => $"uOrgHub password reset code: {code}. Expires in 10 minutes.",
            "PhoneVerify" => $"uOrgHub phone verification code: {code}. Expires in 10 minutes.",
            _ => $"uOrgHub verification code: {code}. Expires in 10 minutes.",
        };

        await MessageResource.CreateAsync(
            body: text,
            from: new Twilio.Types.PhoneNumber(_config["SmsSettings:FromNumber"]!),
            to: new Twilio.Types.PhoneNumber(phoneNumber));
    }
}

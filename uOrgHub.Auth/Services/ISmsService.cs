namespace uOrgHub.Auth.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string code, string purpose);
}

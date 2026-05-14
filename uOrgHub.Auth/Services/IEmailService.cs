namespace uOrgHub.Auth.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
    Task SendOtpAsync(string to, string code, string purpose);
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace uOrgHub.Sms.Services;

/// <summary>Encrypts a gateway's secret settings for storage. Plain values never leave this class except to a provider call.</summary>
public interface ISmsCredentialProtector
{
    string? Protect(IReadOnlyDictionary<string, string> secrets);
    Dictionary<string, string> Unprotect(string? cipher);
}

/// <summary>
/// AES-256-GCM over the secrets serialised as JSON; stored as "v1:" + base64(nonce | tag | ciphertext).
///
/// The key comes from <c>Sms:CredentialKey</c> (env <c>Sms__CredentialKey</c>), any string of 32+
/// characters, hashed to 256 bits. Without one it falls back to a key derived from the JWT signing
/// secret and warns at startup: that works, but rotating the JWT secret would then make every
/// stored SMS credential unreadable, so production should set its own key. Losing or changing the
/// key means re-entering the gateway credentials — nothing else is affected.
/// </summary>
public sealed class SmsCredentialProtector : ISmsCredentialProtector
{
    private const string Prefix = "v1:";
    private const int NonceSize = 12, TagSize = 16;
    private readonly byte[] _key;

    public SmsCredentialProtector(IConfiguration config, ILogger<SmsCredentialProtector> logger)
    {
        var configured = config["Sms:CredentialKey"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (configured.Length < 32)
                throw new InvalidOperationException("Sms:CredentialKey must be at least 32 characters.");
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(configured));
            return;
        }

        var jwt = config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("Set Sms:CredentialKey (or JwtSettings:SecretKey) so SMS gateway credentials can be encrypted.");
        logger.LogWarning("Sms:CredentialKey is not set; SMS gateway credentials are encrypted with a key derived from the JWT secret. Set Sms__CredentialKey in production.");
        _key = SHA256.HashData(Encoding.UTF8.GetBytes("uOrgHub.Sms.Credentials|" + jwt));
    }

    /// <summary>For tests.</summary>
    public SmsCredentialProtector(byte[] key) => _key = key;

    public string? Protect(IReadOnlyDictionary<string, string> secrets)
    {
        if (secrets.Count == 0) return null;
        var plain = JsonSerializer.SerializeToUtf8Bytes(secrets);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var cipher = new byte[plain.Length];
        using (var aes = new AesGcm(_key, TagSize))
            aes.Encrypt(nonce, plain, cipher, tag);
        CryptographicOperations.ZeroMemory(plain);
        return Prefix + Convert.ToBase64String([.. nonce, .. tag, .. cipher]);
    }

    public Dictionary<string, string> Unprotect(string? cipher)
    {
        if (string.IsNullOrEmpty(cipher)) return new();
        if (!cipher.StartsWith(Prefix))
            throw new CryptographicException("Unrecognised SMS credential format.");

        var data = Convert.FromBase64String(cipher[Prefix.Length..]);
        var nonce = data.AsSpan(0, NonceSize);
        var tag = data.AsSpan(NonceSize, TagSize);
        var body = data.AsSpan(NonceSize + TagSize);
        var plain = new byte[body.Length];
        using (var aes = new AesGcm(_key, TagSize))
            aes.Decrypt(nonce, body, tag, plain);
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(plain) ?? new();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plain);
        }
    }
}

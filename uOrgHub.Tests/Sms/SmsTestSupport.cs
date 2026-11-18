using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Services;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Providers;
using uOrgHub.Sms.Services;

namespace uOrgHub.Tests.Sms;

/// <summary>A provider whose answers the test scripts, recording what it was asked to send.</summary>
public sealed class FakeSmsProvider : ISmsProvider
{
    public Queue<Func<SmsOutgoingMessage, SmsSendResult>> Script { get; } = new();
    public List<SmsOutgoingMessage> Sent { get; } = new();
    public SmsBalanceResult Balance { get; set; } = new(true, true, 500m, "BDT");
    public SmsDeliveryResult Delivery { get; set; } = SmsDeliveryResult.NotSupported;

    public SmsProviderDescriptor Descriptor { get; } = new(
        "fake", "Fake", "Test double", "https://fake.test",
        [new SmsProviderField("api_key", "API key", IsSecret: true, Required: true), new SmsProviderField("region", "Region", DefaultValue: "bd")],
        new SmsProviderCapabilities(Balance: true, Rate: false, DeliveryReports: true, RequiresSenderId: false));

    public Task<SmsSendResult> SendAsync(SmsProviderContext context, SmsOutgoingMessage message, CancellationToken ct)
    {
        Sent.Add(message);
        var next = Script.Count > 0 ? Script.Dequeue() : _ => new SmsSendResult(SmsSendOutcome.Accepted, ProviderMessageId: "P-" + Sent.Count);
        return Task.FromResult(next(message));
    }

    public Task<SmsBalanceResult> GetBalanceAsync(SmsProviderContext context, CancellationToken ct) => Task.FromResult(Balance);

    public Task<SmsDeliveryResult> GetDeliveryStatusAsync(SmsProviderContext context, string id, string recipient, CancellationToken ct)
        => Task.FromResult(Delivery);
}

/// <summary>Answers every HTTP call with one canned response and keeps the requests.</summary>
public sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<(HttpRequestMessage Request, string? Body)> Requests { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
        Requests.Add((request, body));
        return respond(request);
    }

    public static StubHttpHandler Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(_ => new HttpResponseMessage(status) { Content = new StringContent(json) });
}

public sealed class StubHttpClientFactory(HttpMessageHandler? handler = null) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(handler ?? StubHttpHandler.Json("{}"));
}

public sealed class FixedCompany(Guid? id) : ICurrentCompanyAccessor
{
    public Guid? CompanyId { get; } = id;
}

public sealed class SmsHarness
{
    public AppDbContext Db { get; } = TestDb.NewContext("TestDb_Sms_" + Guid.NewGuid());
    public FakeSmsProvider Fake { get; } = new();
    public ISmsCredentialProtector Protector { get; } = new SmsCredentialProtector(new byte[32]);
    public SmsDispatchSignal Signal { get; } = new();
    public SmsProviderRegistry Registry { get; }
    public SmsGatewayRuntime Runtime { get; }

    public SmsHarness()
    {
        Registry = new SmsProviderRegistry([Fake, new LogOnlySmsProvider(NullLogger<LogOnlySmsProvider>.Instance)]);
        Runtime = new SmsGatewayRuntime(Registry, Protector, new StubHttpClientFactory());
    }

    public SmsSendService SendService() => new(Db, Signal, new FixedCompany(Guid.NewGuid()));
    public SmsDispatcher Dispatcher() => new(Db, Runtime, Registry, NullLogger<SmsDispatcher>.Instance);

    public SmsGateway AddGateway(string name = "Fake GW", bool isDefault = true, bool enabled = true, decimal? rate = 0.35m)
    {
        var gateway = new SmsGateway
        {
            Name = name,
            ProviderKey = "fake",
            IsDefault = isDefault,
            IsEnabled = enabled,
            RatePerSegment = rate,
            MaxSendsPerSecond = 100, // no throttle delay in tests
            CredentialsCipher = Protector.Protect(new Dictionary<string, string> { ["api_key"] = "secret-key-123" }),
        };
        Db.Set<SmsGateway>().Add(gateway);
        Db.SaveChanges();
        return gateway;
    }
}

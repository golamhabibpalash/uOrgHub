using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Providers;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Gateways.Queries;

/// <summary>All of the company's gateways — a handful at most, so not paged.</summary>
public record GetSmsGatewaysQuery : IQuery<List<SmsGatewayResponseDto>>;
public record GetSmsGatewayByIdQuery(Guid Id) : IQuery<SmsGatewayResponseDto>;
public record GetSmsProvidersQuery : IQuery<IReadOnlyList<SmsProviderDescriptor>>;

public class GetSmsGatewaysQueryHandler(AppDbContext db, ISmsGatewayRuntime runtime)
    : IRequestHandler<GetSmsGatewaysQuery, List<SmsGatewayResponseDto>>
{
    public async Task<List<SmsGatewayResponseDto>> Handle(GetSmsGatewaysQuery request, CancellationToken ct)
    {
        var gateways = await db.Set<SmsGateway>().AsNoTracking()
            .Where(g => !g.IsDeleted)
            .OrderByDescending(g => g.IsDefault).ThenBy(g => g.Name)
            .ToListAsync(ct);
        return gateways.Select(runtime.ToDto).ToList();
    }
}

public class GetSmsGatewayByIdQueryHandler(AppDbContext db, ISmsGatewayRuntime runtime)
    : IRequestHandler<GetSmsGatewayByIdQuery, SmsGatewayResponseDto>
{
    public async Task<SmsGatewayResponseDto> Handle(GetSmsGatewayByIdQuery request, CancellationToken ct)
    {
        var gateway = await db.Set<SmsGateway>().AsNoTracking()
            .FirstOrDefaultAsync(g => !g.IsDeleted && g.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SmsGateway), request.Id);
        return runtime.ToDto(gateway);
    }
}

public class GetSmsProvidersQueryHandler(ISmsProviderRegistry registry)
    : IRequestHandler<GetSmsProvidersQuery, IReadOnlyList<SmsProviderDescriptor>>
{
    public Task<IReadOnlyList<SmsProviderDescriptor>> Handle(GetSmsProvidersQuery request, CancellationToken ct)
        => Task.FromResult(registry.Descriptors);
}

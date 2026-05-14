using MediatR;

namespace uOrgHub.Procurement.Features._Common;

public interface ICommand<TResult> : IRequest<TResult> { }
public interface IQuery<TResult> : IRequest<TResult> { }

internal record VariantInfo(string SKU, string VariantName);

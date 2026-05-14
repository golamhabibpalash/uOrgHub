using MediatR;

namespace uOrgHub.Inventory.Features._Common;

public interface ICommand<TResult> : IRequest<TResult> { }
public interface IQuery<TResult> : IRequest<TResult> { }

using MediatR;

namespace uOrgHub.Accounts.Features._Common;

public interface ICommand<TResult> : IRequest<TResult> { }
public interface IQuery<TResult> : IRequest<TResult> { }

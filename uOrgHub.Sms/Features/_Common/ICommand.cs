using MediatR;

namespace uOrgHub.Sms.Features._Common;

public interface ICommand<TResult> : IRequest<TResult> { }
public interface IQuery<TResult> : IRequest<TResult> { }

using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using uOrgHub.Shared.Behaviors;

namespace uOrgHub.Tests.Shared;

// Public top-level doubles: FluentValidation's assembly scan skips nested/private types,
// so these must be visible for AddValidatorsFromAssembly to register them.
public record ValidationBehaviorTestDto(string Name);
public record ValidationBehaviorTestCommand(ValidationBehaviorTestDto Dto) : IRequest<string>;
public record ValidationBehaviorBareCommand(string Name) : IRequest<string>;

public class ValidationBehaviorTestCommandHandler : IRequestHandler<ValidationBehaviorTestCommand, string>
{
    public Task<string> Handle(ValidationBehaviorTestCommand request, CancellationToken ct) => Task.FromResult("ok");
}

public class ValidationBehaviorBareCommandHandler : IRequestHandler<ValidationBehaviorBareCommand, string>
{
    public Task<string> Handle(ValidationBehaviorBareCommand request, CancellationToken ct) => Task.FromResult("ok");
}

public class ValidationBehaviorTestDtoValidator : AbstractValidator<ValidationBehaviorTestDto>
{
    public ValidationBehaviorTestDtoValidator() => RuleFor(x => x.Name).NotEmpty();
}

public class ValidationBehaviorBareCommandValidator : AbstractValidator<ValidationBehaviorBareCommand>
{
    public ValidationBehaviorBareCommandValidator() => RuleFor(x => x.Name).NotEmpty();
}

public class ValidationBehaviorTests
{
    private static IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ValidationBehaviorTests).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(ValidationBehaviorTests).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_with_invalid_nested_dto_throws_before_handler_runs()
    {
        var mediator = CreateMediator();
        var act = () => mediator.Send(new ValidationBehaviorTestCommand(new ValidationBehaviorTestDto("")));
        await act.Should().ThrowAsync<uOrgHub.Shared.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Send_with_valid_nested_dto_reaches_handler()
    {
        var mediator = CreateMediator();
        (await mediator.Send(new ValidationBehaviorTestCommand(new ValidationBehaviorTestDto("ok")))).Should().Be("ok");
    }

    [Fact]
    public async Task Send_with_invalid_request_itself_throws()
    {
        var mediator = CreateMediator();
        var act = () => mediator.Send(new ValidationBehaviorBareCommand(""));
        await act.Should().ThrowAsync<uOrgHub.Shared.Exceptions.ValidationException>();
    }
}

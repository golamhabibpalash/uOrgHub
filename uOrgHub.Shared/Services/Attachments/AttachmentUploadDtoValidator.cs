using FluentValidation;

namespace uOrgHub.Shared.Services.Attachments;

public class AttachmentUploadDtoValidator : AbstractValidator<AttachmentUploadDto>
{
    public AttachmentUploadDtoValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(100);

        RuleFor(x => x.EntityId).NotEmpty();

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddAvatar;

public class AddAvatarCommandValidator : AbstractValidator<AddAvatarCommand>
{
    public AddAvatarCommandValidator()
    {
        RuleFor(s => s.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsInvalid("user id"));

        RuleFor(p => p.UploadFileDto.FileName)
            .Must(ext => Constraints.Extensions.Contains(Path.GetExtension(ext)))
            .NotEmpty().WithError(Error.Null(
                "filename.is.null",
                "filename cannot be null or empty"));

        RuleFor(p => p.UploadFileDto.BucketName)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("BucketName"));

        RuleFor(p => p.UploadFileDto.ContentType)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("ContentType"));
    }
}
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation.Results;

namespace AnimalAllies.Core.Extension;

public static class ValidationExtension
{
    public static ErrorList ToErrorList(this ValidationResult validationResult)
    {
        List<ValidationFailure>? validationErrors = validationResult.Errors;

        // TODO: Ошибка десериализации, пофиксить позже
        IEnumerable<Error> errors = from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = Error.Deserialize(errorMessage)
            select Error.Validation(error.ErrorCode, error.ErrorMessage, validationError.PropertyName);

        return new ErrorList(errors);
    }
}
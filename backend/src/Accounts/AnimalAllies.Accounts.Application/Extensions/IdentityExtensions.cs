using AnimalAllies.SharedKernel.Shared.Errors;
using Microsoft.AspNetCore.Identity;

namespace AnimalAllies.Accounts.Application.Extensions;

public static class IdentityExtensions
{
    public static ErrorList ToErrorList(this IEnumerable<IdentityError> identityErrors)
    {
        IEnumerable<Error> errors = identityErrors.Select(ie => Error.Failure(ie.Code, ie.Description));

        return new ErrorList(errors);
    }
}
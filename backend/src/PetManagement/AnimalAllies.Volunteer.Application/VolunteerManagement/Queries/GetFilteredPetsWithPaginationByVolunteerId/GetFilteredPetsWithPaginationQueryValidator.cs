﻿using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetFilteredPetsWithPaginationByVolunteerId;

public class GetFilteredPetsWithPaginationQueryValidator : AbstractValidator<GetFilteredPetsWithPaginationQuery>
{
    public GetFilteredPetsWithPaginationQueryValidator()
    {
        RuleFor(v => v.VolunteerId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("volunteer id"));
    }
}

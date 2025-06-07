using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;
using AutoFixture;

namespace AnimalAllies.Volunteer.IntegrationTests.Application;

public static class FixtureExtensions
{
    public static CreateVolunteerCommand CreateVolunteerCommand(this Fixture fixture)
    {
        return fixture.Build<CreateVolunteerCommand>()
            .With(c => c.FullName, new FullNameDto("string","string","string"))
            .With(c => c.WorkExperience, 30)
            .With(c => c.PhoneNumber, "+12345678910")
            .With(c => c.Email, "admin@example.com")
            .Create();
    }
}
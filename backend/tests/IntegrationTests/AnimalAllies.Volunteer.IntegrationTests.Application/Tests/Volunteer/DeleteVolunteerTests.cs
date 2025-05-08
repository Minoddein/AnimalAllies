using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeleteVolunteer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer;

public class DeleteVolunteerTests : VolunteerTestsBase
{
    private readonly ICommandHandler<DeleteVolunteerCommand, VolunteerId> _sut;

    public DeleteVolunteerTests(IntegrationTestsWebFactory factory)
        : base(factory) => _sut =
        _scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteVolunteerCommand, VolunteerId>>();

    [Fact]
    public async Task Delete_volunteer()
    {
        // Arrange
        Domain.VolunteerManagement.Aggregate.Volunteer volunteer = SeedVolunteer();

        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        DeleteVolunteerCommand command = new(volunteer.Id.Id);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        Domain.VolunteerManagement.Aggregate.Volunteer? isVolunteerExist =
            await _volunteerDbContext.Volunteers.FirstOrDefaultAsync(v => v.Id == volunteer.Id).ConfigureAwait(false);

        isVolunteerExist.Should().NotBeNull();
        isVolunteerExist.IsDeleted.Should().BeTrue();
    }

    private Domain.VolunteerManagement.Aggregate.Volunteer SeedVolunteer()
    {
        CreateVolunteerCommand command = _fixture.CreateVolunteerCommand();

        VolunteerId volunteerId = VolunteerId.NewGuid();

        FullName fullName = FullName
            .Create(
                command.FullName.FirstName,
                command.FullName.SecondName,
                command.FullName.Patronymic).Value;

        ValueObjectList<Requisite> requisites = new(command.Requisites.Select(r =>
            Requisite.Create(r.Title, r.Description).Value));

        Domain.VolunteerManagement.Aggregate.Volunteer volunteer = new(
            volunteerId,
            fullName,
            Email.Create(command.Email).Value,
            VolunteerDescription.Create(command.Description).Value,
            WorkExperience.Create(command.WorkExperience).Value,
            PhoneNumber.Create(command.PhoneNumber).Value,
            requisites);

        return volunteer;
    }
}
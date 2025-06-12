using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeleteVolunteer;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer;

public class DeleteVolunteerTests: VolunteerTestsBase
{
    private ICommandHandler<DeleteVolunteerCommand, VolunteerId> _sut;
    
    public DeleteVolunteerTests(IntegrationTestsWebFactory factory) : base(factory)
    {
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<DeleteVolunteerCommand, VolunteerId>>();
    }
    
    [Fact]
    public async Task Delete_volunteer()
    {
        //Arrange
        var volunteer = SeedVolunteer();

        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();
        
        var command = new DeleteVolunteerCommand(volunteer.Id.Id);
        
        //Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var isVolunteerExist = await _volunteerDbContext.Volunteers.FirstOrDefaultAsync(v => v.Id == volunteer.Id);

        isVolunteerExist.Should().NotBeNull();
        isVolunteerExist.IsDeleted.Should().BeTrue();
    }

    private Domain.VolunteerManagement.Aggregate.Volunteer SeedVolunteer()
    {
        var command = _fixture.CreateVolunteerCommand();

        var volunteerId = VolunteerId.NewGuid();

        var fullName = FullName
            .Create(
                command.FullName.FirstName,
                command.FullName.SecondName,
                command.FullName.Patronymic).Value;

        var requisites = new ValueObjectList<Requisite>(command.Requisites.Select(r => 
            Requisite.Create(r.Title, r.Description).Value));

        var volunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
            volunteerId,
            fullName,
            Email.Create(command.Email).Value,
            VolunteerDescription.Create(command.Description).Value,
            WorkExperience.Create(command.WorkExperience).Value,
            PhoneNumber.Create(command.PhoneNumber).Value,
            Relation.Create(Guid.NewGuid()).Value,
            requisites,
            new ValueObjectList<Skill>([]));

        return volunteer;
    }
}
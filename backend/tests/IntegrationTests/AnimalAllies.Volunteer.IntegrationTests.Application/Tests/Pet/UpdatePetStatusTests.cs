using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePetStatus;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;
//test
public class UpdatePetStatusTests : VolunteerTestsBase
{
    private readonly ICommandHandler<UpdatePetStatusCommand, PetId> _sut;
    private readonly IntegrationTestsWebFactory _factory;

    public UpdatePetStatusTests(IntegrationTestsWebFactory factory) : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdatePetStatusCommand, PetId>>();
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldUpdateStatus_WhenValidDataProvided()
    {
        // Arrange
        var species = new Species.Domain.Species(SpeciesId.Create(Guid.NewGuid()), Name.Create("Dog").Value);
        species.AddBreed(new Breed(BreedId.Create(Guid.NewGuid()), Name.Create("Labrador").Value));

        await _speciesDbContext.Species.AddAsync(species);
        await _speciesDbContext.SaveChangesAsync();
        
        _factory.SetupSuccessSpeciesContractsMock(species.Id.Id, species.Breeds[0].Id.Id);
        
        var volunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            Relation.Create(Guid.NewGuid()).Value,
            new ValueObjectList<Requisite>([]));

        var petId = PetId.NewGuid();
        var initialPet = new Domain.VolunteerManagement.Entities.Pet.Pet(
            petId,
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Черный", "Здоров", 
                4.5f, 0.4f, false, false).Value,
            PetDetails.Create("Спокойный кот",
                DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateTime.UtcNow).Value,
            Address.Create("ул. Пушкина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>([]));

        volunteer.AddPet(initialPet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var command = new UpdatePetStatusCommand(
            VolunteerId: volunteer.Id.Id,
            PetId: petId.Id,
            HelpStatus: "FoundHome");
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(petId);
        
        var updatedVolunteer = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id);
            
        updatedVolunteer.Should().NotBeNull();
        var updatedPet = updatedVolunteer.Pets.First();
        updatedPet.HelpStatus.Value.Should().Be("FoundHome");
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldReturnError_WhenVolunteerNotFound()
    {
        // Arrange
        var nonExistentVolunteerId = Guid.NewGuid();
        var command = new UpdatePetStatusCommand(
            VolunteerId: nonExistentVolunteerId,
            PetId: Guid.NewGuid(),
            HelpStatus: "FoundHome");
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldReturnError_WhenPetNotFound()
    {
        // Arrange
        var volunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            Relation.Create(Guid.NewGuid()).Value,
            new ValueObjectList<Requisite>([]));

        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var nonExistentPetId = Guid.NewGuid();
        var command = new UpdatePetStatusCommand(
            VolunteerId: volunteer.Id.Id,
            PetId: nonExistentPetId,
            HelpStatus: "FoundHome");
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldReturnError_WhenInvalidStatusProvided()
    {
        // Arrange
        var species = new Species.Domain.Species(SpeciesId.Create(Guid.NewGuid()), Name.Create("Dog").Value);
        species.AddBreed(new Breed(BreedId.Create(Guid.NewGuid()), Name.Create("Labrador").Value));

        await _speciesDbContext.Species.AddAsync(species);
        await _speciesDbContext.SaveChangesAsync();
        
        _factory.SetupSuccessSpeciesContractsMock(species.Id.Id, species.Breeds[0].Id.Id);
        
        var volunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            Relation.Create(Guid.NewGuid()).Value,
            new ValueObjectList<Requisite>([]));

        var petId = PetId.NewGuid();
        var initialPet = new Domain.VolunteerManagement.Entities.Pet.Pet(
            petId,
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Черный", "Здоров", 
                4.5f, 0.4f, false, false).Value,
            PetDetails.Create("Спокойный кот",
                DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateTime.UtcNow).Value,
            Address.Create("ул. Пушкина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>([]));

        volunteer.AddPet(initialPet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var command = new UpdatePetStatusCommand(
            VolunteerId: volunteer.Id.Id,
            PetId: petId.Id,
            HelpStatus: "InvalidStatus");
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
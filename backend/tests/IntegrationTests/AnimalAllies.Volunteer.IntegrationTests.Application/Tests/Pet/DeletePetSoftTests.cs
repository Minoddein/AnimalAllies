using System.Reflection;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetSoft;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;

public class DeletePetSoftTests : VolunteerTestsBase
{
    private readonly ICommandHandler<DeletePetSoftCommand, PetId> _sut;
    private readonly IntegrationTestsWebFactory _factory;

    public DeletePetSoftTests(IntegrationTestsWebFactory factory) : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<DeletePetSoftCommand, PetId>>();
    }

    [Fact]
    public async Task DeletePetSoft_ShouldMarkPetAsDeleted_WhenPetExists()
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

        var pet = new Domain.VolunteerManagement.Entities.Pet.Pet(
            PetId.NewGuid(),
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Рыжий", "Здоров", 5.2f, 0.5f, true, true).Value,
            PetDetails.Create("Дружелюбный кот", DateOnly.FromDateTime(DateTime.Now.AddYears(-2)), DateTime.UtcNow).Value,
            Address.Create("ул. Ленина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>(new[]
            {
                Requisite.Create("Паспорт", "Серия 1234 №567890").Value
            }));

        volunteer.AddPet(pet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var command = new DeletePetSoftCommand(volunteer.Id.Id, pet.Id.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(pet.Id);

        var volunteerFromDb = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync();

        var updatedPet = volunteerFromDb!.Pets[0];
            
        updatedPet.Should().NotBeNull();
        updatedPet.IsDeleted.Should().BeTrue();
        updatedPet.DeletionDate.Should().NotBeNull();
        updatedPet.DeletionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeletePetSoft_ShouldReturnNotFound_WhenVolunteerNotExists()
    {
        // Arrange
        var nonExistentVolunteerId = Guid.NewGuid();
        var command = new DeletePetSoftCommand(nonExistentVolunteerId, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task DeletePetSoft_ShouldReturnNotFound_WhenPetNotExists()
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
        var command = new DeletePetSoftCommand(volunteer.Id.Id, nonExistentPetId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task DeletePetSoft_ShouldReturnValidationError_WhenInvalidCommand()
    {
        // Arrange
        var invalidCommand = new DeletePetSoftCommand(Guid.Empty, Guid.Empty);

        // Act
        var result = await _sut.Handle(invalidCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("volunteer id"));
    }
}
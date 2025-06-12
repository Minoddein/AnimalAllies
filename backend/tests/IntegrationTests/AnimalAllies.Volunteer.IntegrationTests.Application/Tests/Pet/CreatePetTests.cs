using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;

public class AddPetTests : VolunteerTestsBase
{
    private readonly ICommandHandler<AddPetCommand, Guid> _sut;
    private readonly IntegrationTestsWebFactory _factory;

    public AddPetTests(IntegrationTestsWebFactory factory) : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<AddPetCommand, Guid>>();
        factory.SetupSuccessSpeciesContractsMock(Guid.NewGuid(), Guid.NewGuid());
    }

    [Fact]
    public async Task AddPet_ShouldCreatePetAndAddToVolunteer_WhenValidDataProvided()
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
            new ValueObjectList<Requisite>([]),
            new ValueObjectList<Skill>([]));
        
        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var volunteerId = volunteer.Id.Id;
        
        var command = new AddPetCommand(
            VolunteerId: volunteerId,
            Name: "Барсик",
            PhoneNumber: "+79998887766",
            HelpStatus: "NeedsHelp",
            PetPhysicCharacteristics: new PetPhysicCharacteristicsDto(
                Color: "Рыжий",
                HealthInformation: "Здоров",
                Weight: 5.2f,
                Height: 0.5f,
                IsCastrated: true,
                IsVaccinated: true),
            PetDetails: new PetDetailsDto(
                Description: "Дружелюбный кот",
                BirthDate: new DateTime(2020, 5, 10)),
            Address: new AddressDto(
                Street: "ул. Ленина",
                City: "Москва",
                State: "Московская область",
                ZipCode: "123456"),
            AnimalType: new AnimalTypeDto(
                SpeciesId: species.Id.Id,
                BreedId: species.Breeds!.FirstOrDefault()!.Id.Id),
            Requisites: new List<RequisiteDto>
            {
                new()
                {
                    Title = "Паспорт",
                    Description = "Серия 1234 №567890"
                }
            });
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        var savedVolunteer = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id);
            
        savedVolunteer.Should().NotBeNull();
        savedVolunteer.Pets.Should().HaveCount(1);
        
        var pet = savedVolunteer.Pets.First();
        pet.Name.Value.Should().Be("Барсик");
        pet.PhoneNumber.Number.Should().Be("+79998887766");
        pet.HelpStatus.Value.Should().Be("NeedsHelp");
        pet.PetPhysicCharacteristics.Color.Should().Be("Рыжий");
        pet.PetDetails.Description.Should().Be("Дружелюбный кот");
        pet.Address.Street.Should().Be("ул. Ленина");
        pet.AnimalType.SpeciesId.Id.Should().Be(species.Id.Id);
        pet.AnimalType.BreedId.Should().Be(species.Breeds.FirstOrDefault()!.Id.Id);
        pet.Requisites.Should().HaveCount(1);
        pet.PetDetails.BirthDate.Should().Be(DateOnly.FromDateTime(new DateTime(2020, 5, 10)));
    }

    [Fact]
    public async Task AddPet_ShouldReturnNotFound_WhenVolunteerNotExists()
    {
        // Arrange
        var species = new Species.Domain.Species(SpeciesId.Create(Guid.NewGuid()), Name.Create("Dog").Value);
        species.AddBreed(new Breed(BreedId.Create(Guid.NewGuid()), Name.Create("Labrador").Value));

        await _speciesDbContext.Species.AddAsync(species);
        await _speciesDbContext.SaveChangesAsync();
        
        _factory.SetupSuccessSpeciesContractsMock(species.Id.Id, species.Breeds[0].Id.Id);
        
        var nonExistentVolunteerId = Guid.NewGuid();
        var command = new AddPetCommand(
            VolunteerId: nonExistentVolunteerId,
            Name: "Барсик",
            PhoneNumber: "+79998887766",
            HelpStatus: "NeedsHelp",
            PetPhysicCharacteristics: new PetPhysicCharacteristicsDto(
                Color: "Рыжий",
                HealthInformation: "Здоров",
                Weight: 5.2f,
                Height: 0.5f,
                IsCastrated: true,
                IsVaccinated: true),
            PetDetails: new PetDetailsDto(
                Description: "Дружелюбный кот",
                BirthDate: DateTime.Now.AddYears(-2)),
            Address: new AddressDto(
                Street: "ул. Ленина",
                City: "Москва",
                State: "Московская область",
                ZipCode: "123456"),
            AnimalType: new AnimalTypeDto(
                SpeciesId: species.Id.Id,
                BreedId: species.Breeds[0].Id.Id),
            Requisites: new List<RequisiteDto>());
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task AddPet_ShouldReturnError_WhenSpeciesNotFound()
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
            new ValueObjectList<Requisite>([]),
            new ValueObjectList<Skill>([]));
        
        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var nonExistentSpeciesId = Guid.NewGuid();
        
        var command = new AddPetCommand(
            VolunteerId: volunteer.Id.Id,
            Name: "Барсик",
            PhoneNumber: "+79998887766",
            HelpStatus: "NeedsHelp",
            PetPhysicCharacteristics: new PetPhysicCharacteristicsDto(
                Color: "Рыжий",
                HealthInformation: "Здоров",
                Weight: 5.2f,
                Height: 0.5f,
                IsCastrated: true,
                IsVaccinated: true),
            PetDetails: new PetDetailsDto(
                Description: "Дружелюбный кот",
                BirthDate: DateTime.Now.AddYears(-2)),
            Address: new AddressDto(
                Street: "ул. Ленина",
                City: "Москва",
                State: "Московская область",
                ZipCode: "123456"),
            AnimalType: new AnimalTypeDto(
                SpeciesId: nonExistentSpeciesId,
                BreedId: Guid.NewGuid()),
            Requisites: new List<RequisiteDto>());
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }
}
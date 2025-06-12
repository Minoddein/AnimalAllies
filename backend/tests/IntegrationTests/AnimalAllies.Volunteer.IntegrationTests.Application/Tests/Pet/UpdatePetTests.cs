using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;

public class UpdatePetTests : VolunteerTestsBase
{
    private readonly ICommandHandler<UpdatePetCommand, Guid> _sut;
    private readonly IntegrationTestsWebFactory _factory;

    public UpdatePetTests(IntegrationTestsWebFactory factory) : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdatePetCommand, Guid>>();
    }

    [Fact]
    public async Task UpdatePet_ShouldUpdatePetData_WhenValidDataProvided()
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

        var petId = PetId.NewGuid();
        var initialPet = new Domain.VolunteerManagement.Entities.Pet.Pet(
            petId,
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Черный", "Здоров", 4.5f, 0.4f,
                false, false).Value,
            PetDetails.Create("Спокойный кот", DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateTime.UtcNow).Value, 
            Address.Create("ул. Пушкина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>(new List<Requisite>
            {
                Requisite.Create("Паспорт", "Серия 1234 №567890").Value
            }));

        volunteer.AddPet(initialPet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer);
        await _volunteerDbContext.SaveChangesAsync();

        var command = new UpdatePetCommand(
            VolunteerId: volunteer.Id.Id,
            PetId: petId.Id,
            Name: "Рыжик",
            PhoneNumber: "+79998887755",
            HelpStatus: "FoundHome",
            PetPhysicCharacteristicsDto: new PetPhysicCharacteristicsDto(
                Color: "Рыжий",
                HealthInformation: "Аллергия на курицу",
                Weight: 5.2f,
                Height: 0.5f,
                IsCastrated: true,
                IsVaccinated: true),
            PetDetailsDto: new PetDetailsDto(
                Description: "Очень активный кот",
                BirthDate: new DateTime(2020, 5, 10)),
            AddressDto: new AddressDto(
                Street: "ул. Ленина",
                City: "Москва",
                State: "Московская область",
                ZipCode: "123456"),
            AnimalTypeDto: new AnimalTypeDto(
                SpeciesId: species.Id.Id,
                BreedId: species.Breeds[0].Id.Id),
            RequisiteDtos: new List<RequisiteDto>
            {
                new()
                {
                    Title = "Чип",
                    Description = "1234567890"
                }
            });
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(petId.Id);
        
        var updatedVolunteer = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id);
            
        updatedVolunteer.Should().NotBeNull();
        updatedVolunteer.Pets.Should().HaveCount(1);
        
        var updatedPet = updatedVolunteer.Pets.First();
        updatedPet.Id.Should().Be(petId);
        updatedPet.Name.Value.Should().Be("Рыжик");
        updatedPet.PhoneNumber.Number.Should().Be("+79998887755");
        updatedPet.HelpStatus.Value.Should().Be("FoundHome");
        updatedPet.PetPhysicCharacteristics.Color.Should().Be("Рыжий");
        updatedPet.PetPhysicCharacteristics.HealthInformation.Should().Be("Аллергия на курицу");
        updatedPet.PetDetails.Description.Should().Be("Очень активный кот");
        updatedPet.Address.Street.Should().Be("ул. Ленина");
        updatedPet.AnimalType.SpeciesId.Should().Be(species.Id);
        updatedPet.AnimalType.BreedId.Should().Be(species.Breeds[0].Id.Id);
        updatedPet.Requisites.Should().HaveCount(1);
        updatedPet.Requisites.First().Title.Should().Be("Чип");
        updatedPet.PetDetails.BirthDate.Should().Be(DateOnly.FromDateTime(new DateTime(2020, 5, 10)));
    }
}
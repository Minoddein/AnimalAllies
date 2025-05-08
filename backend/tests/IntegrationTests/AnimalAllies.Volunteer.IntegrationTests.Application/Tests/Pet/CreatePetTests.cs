using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;

public class AddPetTests : VolunteerTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;
    private readonly ICommandHandler<AddPetCommand, Guid> _sut;

    public AddPetTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<AddPetCommand, Guid>>();
        factory.SetupSuccessSpeciesContractsMock(Guid.NewGuid(), Guid.NewGuid());
    }

    [Fact]
    public async Task AddPet_ShouldCreatePetAndAddToVolunteer_WhenValidDataProvided()
    {
        // Arrange
        Species.Domain.Species species = new(SpeciesId.Create(Guid.NewGuid()), Name.Create("Dog").Value);
        species.AddBreed(new Breed(BreedId.Create(Guid.NewGuid()), Name.Create("Labrador").Value));

        await _speciesDbContext.Species.AddAsync(species).ConfigureAwait(false);
        await _speciesDbContext.SaveChangesAsync().ConfigureAwait(false);

        _factory.SetupSuccessSpeciesContractsMock(species.Id.Id, species.Breeds[0].Id.Id);

        Domain.VolunteerManagement.Aggregate.Volunteer volunteer = new(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            new ValueObjectList<Requisite>([]));

        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        Guid volunteerId = volunteer.Id.Id;

        AddPetCommand command = new(
            volunteerId,
            "Барсик",
            PhoneNumber: "+79998887766",
            HelpStatus: "NeedsHelp",
            PetPhysicCharacteristics: new PetPhysicCharacteristicsDto(
                "Рыжий",
                "Здоров",
                5.2f,
                0.5f,
                true,
                true),
            PetDetails: new PetDetailsDto(
                "Дружелюбный кот",
                new DateTime(2020, 5, 10)),
            Address: new AddressDto(
                "ул. Ленина",
                "Москва",
                "Московская область",
                "123456"),
            AnimalType: new AnimalTypeDto(
                species.Id.Id,
                species.Breeds!.FirstOrDefault()!.Id.Id),
            Requisites: [new RequisiteDto { Title = "Паспорт", Description = "Серия 1234 №567890" }]);

        // Act
        Result<Guid> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        Domain.VolunteerManagement.Aggregate.Volunteer? savedVolunteer = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id).ConfigureAwait(false);

        savedVolunteer.Should().NotBeNull();
        savedVolunteer.Pets.Should().HaveCount(1);

        Domain.VolunteerManagement.Entities.Pet.Pet pet = savedVolunteer.Pets.First();
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
        Species.Domain.Species species = new(SpeciesId.Create(Guid.NewGuid()), Name.Create("Dog").Value);
        species.AddBreed(new Breed(BreedId.Create(Guid.NewGuid()), Name.Create("Labrador").Value));

        await _speciesDbContext.Species.AddAsync(species).ConfigureAwait(false);
        await _speciesDbContext.SaveChangesAsync().ConfigureAwait(false);

        _factory.SetupSuccessSpeciesContractsMock(species.Id.Id, species.Breeds[0].Id.Id);

        Guid nonExistentVolunteerId = Guid.NewGuid();
        AddPetCommand command = new(
            nonExistentVolunteerId,
            "Барсик",
            PhoneNumber: "+79998887766",
            HelpStatus: "NeedsHelp",
            PetPhysicCharacteristics: new PetPhysicCharacteristicsDto(
                "Рыжий",
                "Здоров",
                5.2f,
                0.5f,
                true,
                true),
            PetDetails: new PetDetailsDto(
                "Дружелюбный кот",
                DateTime.Now.AddYears(-2)),
            Address: new AddressDto(
                "ул. Ленина",
                "Москва",
                "Московская область",
                "123456"),
            AnimalType: new AnimalTypeDto(
                species.Id.Id,
                species.Breeds[0].Id.Id),
            Requisites: []);

        // Act
        Result<Guid> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task AddPet_ShouldReturnError_WhenSpeciesNotFound()
    {
        // Arrange
        Domain.VolunteerManagement.Aggregate.Volunteer volunteer = new(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            new ValueObjectList<Requisite>([]));

        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        Guid nonExistentSpeciesId = Guid.NewGuid();

        AddPetCommand command = new(
            volunteer.Id.Id,
            "Барсик",
            PhoneNumber: "+79998887766",
            HelpStatus: "NeedsHelp",
            PetPhysicCharacteristics: new PetPhysicCharacteristicsDto(
                "Рыжий",
                "Здоров",
                5.2f,
                0.5f,
                true,
                true),
            PetDetails: new PetDetailsDto(
                "Дружелюбный кот",
                DateTime.Now.AddYears(-2)),
            Address: new AddressDto(
                "ул. Ленина",
                "Москва",
                "Московская область",
                "123456"),
            AnimalType: new AnimalTypeDto(
                nonExistentSpeciesId,
                Guid.NewGuid()),
            Requisites: []);

        // Act
        Result<Guid> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }
}
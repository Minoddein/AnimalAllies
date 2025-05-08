using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePetStatus;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;

// test
public class UpdatePetStatusTests : VolunteerTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;
    private readonly ICommandHandler<UpdatePetStatusCommand, PetId> _sut;

    public UpdatePetStatusTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdatePetStatusCommand, PetId>>();
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldUpdateStatus_WhenValidDataProvided()
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

        PetId petId = PetId.NewGuid();
        Domain.VolunteerManagement.Entities.Pet.Pet initialPet = new(
            petId,
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Черный", "Здоров",
                4.5f, 0.4f, false, false).Value,
            PetDetails.Create(
                "Спокойный кот",
                DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateTime.UtcNow).Value,
            Address.Create("ул. Пушкина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>([]));

        volunteer.AddPet(initialPet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        UpdatePetStatusCommand command = new(
            volunteer.Id.Id,
            petId.Id,
            "FoundHome");

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(petId);

        Domain.VolunteerManagement.Aggregate.Volunteer? updatedVolunteer = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id).ConfigureAwait(false);

        updatedVolunteer.Should().NotBeNull();
        Domain.VolunteerManagement.Entities.Pet.Pet updatedPet = updatedVolunteer.Pets.First();
        updatedPet.HelpStatus.Value.Should().Be("FoundHome");
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldReturnError_WhenVolunteerNotFound()
    {
        // Arrange
        Guid nonExistentVolunteerId = Guid.NewGuid();
        UpdatePetStatusCommand command = new(
            nonExistentVolunteerId,
            Guid.NewGuid(),
            "FoundHome");

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldReturnError_WhenPetNotFound()
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

        Guid nonExistentPetId = Guid.NewGuid();
        UpdatePetStatusCommand command = new(
            volunteer.Id.Id,
            nonExistentPetId,
            "FoundHome");

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task UpdatePetStatus_ShouldReturnError_WhenInvalidStatusProvided()
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

        PetId petId = PetId.NewGuid();
        Domain.VolunteerManagement.Entities.Pet.Pet initialPet = new(
            petId,
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Черный", "Здоров",
                4.5f, 0.4f, false, false).Value,
            PetDetails.Create(
                "Спокойный кот",
                DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateTime.UtcNow).Value,
            Address.Create("ул. Пушкина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>([]));

        volunteer.AddPet(initialPet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        UpdatePetStatusCommand command = new(
            volunteer.Id.Id,
            petId.Id,
            "InvalidStatus");

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
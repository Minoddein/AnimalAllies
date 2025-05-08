using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.Entities;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetSoft;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Pet;

public class DeletePetSoftTests : VolunteerTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;
    private readonly ICommandHandler<DeletePetSoftCommand, PetId> _sut;

    public DeletePetSoftTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _factory = factory;
        _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<DeletePetSoftCommand, PetId>>();
    }

    [Fact]
    public async Task DeletePetSoft_ShouldMarkPetAsDeleted_WhenPetExists()
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

        Domain.VolunteerManagement.Entities.Pet.Pet pet = new(
            PetId.NewGuid(),
            Name.Create("Барсик").Value,
            PetPhysicCharacteristics.Create("Рыжий", "Здоров", 5.2f, 0.5f, true, true).Value,
            PetDetails.Create("Дружелюбный кот", DateOnly.FromDateTime(DateTime.Now.AddYears(-2)), DateTime.UtcNow)
                .Value,
            Address.Create("ул. Ленина", "Москва", "Московская область", "123456").Value,
            PhoneNumber.Create("+79998887766").Value,
            HelpStatus.Create("NeedsHelp").Value,
            new AnimalType(species.Id, species.Breeds[0].Id.Id),
            new ValueObjectList<Requisite>([Requisite.Create("Паспорт", "Серия 1234 №567890").Value]));

        volunteer.AddPet(pet);
        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        DeletePetSoftCommand command = new(volunteer.Id.Id, pet.Id.Id);

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(pet.Id);

        Domain.VolunteerManagement.Aggregate.Volunteer? volunteerFromDb = await _volunteerDbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync().ConfigureAwait(false);

        Domain.VolunteerManagement.Entities.Pet.Pet updatedPet = volunteerFromDb!.Pets[0];

        updatedPet.Should().NotBeNull();
        updatedPet.IsDeleted.Should().BeTrue();
        updatedPet.DeletionDate.Should().NotBeNull();
        updatedPet.DeletionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeletePetSoft_ShouldReturnNotFound_WhenVolunteerNotExists()
    {
        // Arrange
        Guid nonExistentVolunteerId = Guid.NewGuid();
        DeletePetSoftCommand command = new(nonExistentVolunteerId, Guid.NewGuid());

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task DeletePetSoft_ShouldReturnNotFound_WhenPetNotExists()
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
        DeletePetSoftCommand command = new(volunteer.Id.Id, nonExistentPetId);

        // Act
        Result<PetId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task DeletePetSoft_ShouldReturnValidationError_WhenInvalidCommand()
    {
        // Arrange
        DeletePetSoftCommand invalidCommand = new(Guid.Empty, Guid.Empty);

        // Act
        Result<PetId> result = await _sut.Handle(invalidCommand, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("volunteer id"));
    }
}
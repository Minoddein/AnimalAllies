using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestoreVolunteer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer;

public class RestoreVolunteerTests : VolunteerTestsBase
{
    private readonly ICommandHandler<RestoreVolunteerCommand, VolunteerId> _sut;

    public RestoreVolunteerTests(IntegrationTestsWebFactory factory)
        : base(factory) => _sut =
        _scope.ServiceProvider.GetRequiredService<ICommandHandler<RestoreVolunteerCommand, VolunteerId>>();

    [Fact]
    public async Task RestoreVolunteer_ShouldSetIsDeletedToFalse_WhenVolunteerExists()
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

        volunteer.Delete();

        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        await _volunteerDbContext.Volunteers.AddAsync(volunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        RestoreVolunteerCommand command = new(volunteer.Id.Id);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(volunteer.Id);

        Domain.VolunteerManagement.Aggregate.Volunteer? restoredVolunteer = await _volunteerDbContext.Volunteers
            .FirstOrDefaultAsync(v => v.Id == volunteer.Id).ConfigureAwait(false);

        restoredVolunteer.Should().NotBeNull();
        restoredVolunteer.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreVolunteer_ShouldReturnNotFound_WhenVolunteerNotExists()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        RestoreVolunteerCommand command = new(nonExistentId);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
    }

    [Fact]
    public async Task RestoreVolunteer_ShouldReturnValidationError_WhenInvalidCommand()
    {
        // Arrange
        RestoreVolunteerCommand invalidCommand = new(Guid.Empty);

        // Act
        Result<VolunteerId> result = await _sut.Handle(invalidCommand, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("volunteer id"));
    }
}
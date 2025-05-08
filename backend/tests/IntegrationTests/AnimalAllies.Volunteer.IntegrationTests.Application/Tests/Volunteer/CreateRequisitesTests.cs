using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer;

public class CreateRequisitesTests : VolunteerTestsBase
{
    private readonly ICommandHandler<CreateRequisitesCommand, VolunteerId> _sut;

    public CreateRequisitesTests(IntegrationTestsWebFactory factory)
        : base(factory) => _sut =
        _scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateRequisitesCommand, VolunteerId>>();

    [Fact]
    public async Task CreateRequisites_ShouldAddNewRequisites_WhenValidDataProvided()
    {
        // Arrange
        Domain.VolunteerManagement.Aggregate.Volunteer originalVolunteer = new(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            new ValueObjectList<Requisite>([]));

        await _volunteerDbContext.Volunteers.AddAsync(originalVolunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        List<RequisiteDto> requisites =
        [
            new() { Title = "Паспорт", Description = "Серия 1234 №567890" },
            new() { Title = "Медкнижка", Description = "Действительна до 2025 года" }
        ];

        CreateRequisitesCommand command = new(originalVolunteer.Id.Id, requisites);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(originalVolunteer.Id);

        Domain.VolunteerManagement.Aggregate.Volunteer? updatedVolunteer = await _volunteerDbContext.Volunteers
            .FirstOrDefaultAsync(v => v.Id == originalVolunteer.Id).ConfigureAwait(false);

        updatedVolunteer.Should().NotBeNull();
        updatedVolunteer.Requisites.Should().HaveCount(2);

        updatedVolunteer.Requisites[0].Title.Should().Be("Паспорт");
        updatedVolunteer.Requisites[0].Description.Should().Be("Серия 1234 №567890");

        updatedVolunteer.Requisites[1].Title.Should().Be("Медкнижка");
        updatedVolunteer.Requisites[1].Description.Should().Be("Действительна до 2025 года");
    }

    [Fact]
    public async Task CreateRequisites_ShouldReturnNotFound_WhenVolunteerNotExists()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        List<RequisiteDto> requisites =
        [
            new() { Title = "Паспорт", Description = "Серия 1234 №567890" }
        ];

        CreateRequisitesCommand command = new(nonExistentId, requisites);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.FirstOrDefault()!.ErrorCode.Should().Be(Errors.General.NotFound().ErrorCode);
        result.Errors.FirstOrDefault()!.ErrorMessage.Should().Be(Errors.General.NotFound().ErrorMessage);
        result.Errors.FirstOrDefault()!.Type.Should().Be(Errors.General.NotFound().Type);
    }

    [Fact]
    public async Task CreateRequisites_ShouldReturnValidationError_WhenInvalidData()
    {
        Domain.VolunteerManagement.Aggregate.Volunteer originalVolunteer = new(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value,
            new ValueObjectList<Requisite>([]));

        await _volunteerDbContext.Volunteers.AddAsync(originalVolunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        List<RequisiteDto> invalidRequisites =
        [
            new() { Title = string.Empty, Description = "Невалидный реквизит" }
        ];

        CreateRequisitesCommand command = new(originalVolunteer.Id.Id, invalidRequisites);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorCode == Errors.General.ValueIsRequired("title").ErrorCode);
    }
}
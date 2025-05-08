using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer;

public class UpdateVolunteerTests : VolunteerTestsBase
{
    private readonly ICommandHandler<UpdateVolunteerCommand, VolunteerId> _sut;

    public UpdateVolunteerTests(IntegrationTestsWebFactory factory)
        : base(factory) => _sut =
        _scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateVolunteerCommand, VolunteerId>>();

    [Fact]
    public async Task UpdateVolunteer_ShouldUpdateAllFields_WhenValidDataProvided()
    {
        // Arrange
        Domain.VolunteerManagement.Aggregate.Volunteer originalVolunteer = new(
            VolunteerId.NewGuid(),
            FullName.Create("Иван", "Иванов", "Иванович").Value,
            Email.Create("ivan@mail.com").Value,
            VolunteerDescription.Create("Опытный волонтер").Value,
            WorkExperience.Create(3).Value,
            PhoneNumber.Create("+79991234567").Value, new ValueObjectList<Requisite>([]));

        await _volunteerDbContext.Volunteers.AddAsync(originalVolunteer).ConfigureAwait(false);
        await _volunteerDbContext.SaveChangesAsync().ConfigureAwait(false);

        UpdateVolunteerMainInfoDto updateDto = new(
            new FullNameDto("Петр", "Петров", "Петрович"),
            "petr@mail.com",
            "Новый опытный волонтер",
            5,
            "+79998765432");

        UpdateVolunteerCommand command = new(originalVolunteer.Id.Id, updateDto);

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Domain.VolunteerManagement.Aggregate.Volunteer? updatedVolunteer = await _volunteerDbContext.Volunteers
            .FirstOrDefaultAsync(v => v.Id == originalVolunteer.Id).ConfigureAwait(false);

        updatedVolunteer.Should().NotBeNull();
        updatedVolunteer.FullName.FirstName.Should().Be("Петр");
        updatedVolunteer.FullName.SecondName.Should().Be("Петров");
        updatedVolunteer.FullName.Patronymic.Should().Be("Петрович");
        updatedVolunteer.Email.Value.Should().Be("petr@mail.com");
        updatedVolunteer.Phone.Number.Should().Be("+79998765432");
        updatedVolunteer.Description.Value.Should().Be("Новый опытный волонтер");
        updatedVolunteer.WorkExperience.Value.Should().Be(5);
    }
}
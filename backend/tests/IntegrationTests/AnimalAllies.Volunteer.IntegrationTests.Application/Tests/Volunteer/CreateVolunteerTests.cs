using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer;

public class CreateVolunteerTests : VolunteerTestsBase
{
    private readonly ICommandHandler<CreateVolunteerCommand, VolunteerId> _sut;

    public CreateVolunteerTests(IntegrationTestsWebFactory factory)
        : base(factory) => _sut =
        _scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateVolunteerCommand, VolunteerId>>();

    [Fact]
    public async Task Create_volunteer()
    {
        // Arrange
        CreateVolunteerCommand command = _fixture.CreateVolunteerCommand();

        // Act
        Result<VolunteerId> result = await _sut.Handle(command, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        Domain.VolunteerManagement.Aggregate.Volunteer? volunteer =
            await _volunteerDbContext.Volunteers.FirstOrDefaultAsync(v => v.Id == result.Value).ConfigureAwait(false);

        volunteer.Should().NotBeNull();
        volunteer.Id.Should().Be(result.Value);
    }
}
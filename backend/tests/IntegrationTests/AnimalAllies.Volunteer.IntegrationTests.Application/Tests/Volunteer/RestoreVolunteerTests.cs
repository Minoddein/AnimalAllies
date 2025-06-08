using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestoreVolunteer;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer
{
    public class RestoreVolunteerTests : VolunteerTestsBase
    {
        private readonly ICommandHandler<RestoreVolunteerCommand, VolunteerId> _sut;

        public RestoreVolunteerTests(IntegrationTestsWebFactory factory) : base(factory)
        {
            _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<RestoreVolunteerCommand, VolunteerId>>();
        }

        [Fact]
        public async Task RestoreVolunteer_ShouldSetIsDeletedToFalse_WhenVolunteerExists()
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
            
            volunteer.Delete();

            await _volunteerDbContext.SaveChangesAsync();
            
            await _volunteerDbContext.Volunteers.AddAsync(volunteer);
            await _volunteerDbContext.SaveChangesAsync();

            var command = new RestoreVolunteerCommand(volunteer.Id.Id);
            
            // Act
            var result = await _sut.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(volunteer.Id);
            
            var restoredVolunteer = await _volunteerDbContext.Volunteers
                .FirstOrDefaultAsync(v => v.Id == volunteer.Id);
                
            restoredVolunteer.Should().NotBeNull();
            restoredVolunteer.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreVolunteer_ShouldReturnNotFound_WhenVolunteerNotExists()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var command = new RestoreVolunteerCommand(nonExistentId);
            
            // Act
            var result = await _sut.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorCode == Errors.General.NotFound(null).ErrorCode);
        }

        [Fact]
        public async Task RestoreVolunteer_ShouldReturnValidationError_WhenInvalidCommand()
        {
            // Arrange
            var invalidCommand = new RestoreVolunteerCommand(Guid.Empty);
            
            // Act
            var result = await _sut.Handle(invalidCommand, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("volunteer id"));
        }
    }
}
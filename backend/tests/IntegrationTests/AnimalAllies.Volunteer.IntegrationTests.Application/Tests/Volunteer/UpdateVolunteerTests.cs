using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer
{
    public class UpdateVolunteerTests : VolunteerTestsBase
    {
        private readonly ICommandHandler<UpdateVolunteerCommand, VolunteerId> _sut;

        public UpdateVolunteerTests(IntegrationTestsWebFactory factory) : base(factory)
        {
            _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateVolunteerCommand, VolunteerId>>();
        }

        [Fact]
        public async Task UpdateVolunteer_ShouldUpdateAllFields_WhenValidDataProvided()
        {
            // Arrange
            var originalVolunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
                VolunteerId.NewGuid(),
                FullName.Create("Иван", "Иванов", "Иванович").Value,
                Email.Create("ivan@mail.com").Value,
                VolunteerDescription.Create("Опытный волонтер").Value,
                WorkExperience.Create(3).Value,
                PhoneNumber.Create("+79991234567").Value,
                Relation.Create(Guid.NewGuid()).Value,
                new ValueObjectList<Requisite>([]));
            
            await _volunteerDbContext.Volunteers.AddAsync(originalVolunteer);
            await _volunteerDbContext.SaveChangesAsync();

            var updateDto = new UpdateVolunteerMainInfoDto(
                new FullNameDto("Петр", "Петров", "Петрович"),
                5,
                "+79998765432");


            var command = new UpdateVolunteerCommand(originalVolunteer.Id.Id, updateDto);
            
            // Act
            var result = await _sut.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            
            var updatedVolunteer = await _volunteerDbContext.Volunteers
                .FirstOrDefaultAsync(v => v.Id == originalVolunteer.Id);
                
            updatedVolunteer.Should().NotBeNull();
            updatedVolunteer.FullName.FirstName.Should().Be("Петр");
            updatedVolunteer.FullName.SecondName.Should().Be("Петров");
            updatedVolunteer.FullName.Patronymic.Should().Be("Петрович");
            updatedVolunteer.Phone.Number.Should().Be("+79998765432");
            // Вырезано обновление описание из фичи
            /*updatedVolunteer.Description.Value.Should().Be("Новый опытный волонтер");*/
            updatedVolunteer.WorkExperience.Value.Should().Be(5);
        }
    }
}
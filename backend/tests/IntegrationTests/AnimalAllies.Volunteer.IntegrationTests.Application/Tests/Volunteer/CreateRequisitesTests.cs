using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application.Tests.Volunteer
{
    public class CreateRequisitesTests : VolunteerTestsBase
    {
        private readonly ICommandHandler<CreateRequisitesCommand, VolunteerId> _sut;

        public CreateRequisitesTests(IntegrationTestsWebFactory factory) : base(factory)
        {
            _sut = _scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateRequisitesCommand, VolunteerId>>();
        }

        [Fact]
        public async Task CreateRequisites_ShouldAddNewRequisites_WhenValidDataProvided()
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
                new ValueObjectList<Requisite>([]),
                new ValueObjectList<Skill>([]));
            
            await _volunteerDbContext.Volunteers.AddAsync(originalVolunteer);
            await _volunteerDbContext.SaveChangesAsync();

            var requisites = new List<RequisiteDto>
            {
                new RequisiteDto
                {
                    Title = "Паспорт",
                    Description = "Серия 1234 №567890"
                        
                },
                new RequisiteDto
                {
                    Title = "Медкнижка",
                    Description = "Действительна до 2025 года"
                        
                }
            };

            var command = new CreateRequisitesCommand(originalVolunteer.Id.Id, requisites);
            
            // Act
            var result = await _sut.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(originalVolunteer.Id);
            
            var updatedVolunteer = await _volunteerDbContext.Volunteers
                .FirstOrDefaultAsync(v => v.Id == originalVolunteer.Id);
                
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
            var nonExistentId = Guid.NewGuid();
            var requisites = new List<RequisiteDto>
            {
                new RequisiteDto
                {
                    Title = "Паспорт",
                    Description = "Серия 1234 №567890"
                        
                },
            };

            var command = new CreateRequisitesCommand(nonExistentId, requisites);
            
            // Act
            var result = await _sut.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.FirstOrDefault()!.ErrorCode.Should().Be(Errors.General.NotFound().ErrorCode);
            result.Errors.FirstOrDefault()!.ErrorMessage.Should().Be(Errors.General.NotFound().ErrorMessage);
            result.Errors.FirstOrDefault()!.Type.Should().Be(Errors.General.NotFound().Type);
        }

        [Fact]
        public async Task CreateRequisites_ShouldReturnValidationError_WhenInvalidData()
        {
            var originalVolunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
                VolunteerId.NewGuid(),
                FullName.Create("Иван", "Иванов", "Иванович").Value,
                Email.Create("ivan@mail.com").Value,
                VolunteerDescription.Create("Опытный волонтер").Value,
                WorkExperience.Create(3).Value,
                PhoneNumber.Create("+79991234567").Value,
                Relation.Create(Guid.NewGuid()).Value,
                new ValueObjectList<Requisite>([]),
                new ValueObjectList<Skill>([]));
            
            await _volunteerDbContext.Volunteers.AddAsync(originalVolunteer);
            await _volunteerDbContext.SaveChangesAsync();

            var invalidRequisites = new List<RequisiteDto>
            {
                new RequisiteDto
                {
                    Title = "",
                    Description = "Невалидный реквизит"
                        
                }
            };

            var command = new CreateRequisitesCommand(originalVolunteer.Id.Id, invalidRequisites);
            
            // Act
            var result = await _sut.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorCode == Errors.General.ValueIsRequired("title").ErrorCode);
        }
    }
}
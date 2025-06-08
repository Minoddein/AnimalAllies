using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentAssertions;

namespace TestProject.Domain;

public class VolunteerTests
{
    [Fact]
    public void Add_Pet_With_Empty_Pets_Return_Success_Result()
    {
        // arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;

        var volunteer = InitVolunteer();

        var pet = InitPet(birthDate, creationTime);


        // act
        var result = volunteer.AddPet(pet);

        // assert
        var addedPetResult = volunteer.GetPetById(pet.Id);

        result.IsSuccess.Should().BeTrue();
        addedPetResult.IsSuccess.Should().BeTrue();
        addedPetResult.Value.Id.Should().Be(pet.Id);
        addedPetResult.Value.Position.Should().Be(Position.First);
    }

    private static Pet InitPet(DateOnly birthDate, DateTime creationTime)
    {
        var petId = PetId.NewGuid();
        var name = Name.Create("Test").Value;
        var petPhysicCharacteristic = PetPhysicCharacteristics.Create(
            "Test",
            "Test",
            1,
            1,
            false,
            false).Value;
        var petDetails = PetDetails.Create("Test", birthDate, creationTime).Value;
        var address = Address.Create(
            "Test",
            "Test",
            "Test",
            "Test").Value;
        var phoneNumber = PhoneNumber.Create("+12345678910").Value;
        var helpStatus = HelpStatus.NeedsHelp;
        var animalType = new AnimalType(SpeciesId.Empty(), Guid.Empty);
        var requisites = new ValueObjectList<Requisite>([Requisite.Create("Test", "Test").Value]);

        var pet = new Pet(
            petId,
            name,
            petPhysicCharacteristic,
            petDetails,
            address,
            phoneNumber,
            helpStatus,
            animalType,
            requisites);
        return pet;
    }

    private Volunteer AddPetsInVolunteer(Volunteer volunteer, int petsCount, DateOnly birthDate, DateTime creationTime)
    {
        for (var i = 0; i < petsCount; i++)
        {
            var pet = InitPet(birthDate, creationTime);
            volunteer.AddPet(pet);
        }

        return volunteer;
    }

    private static Volunteer InitVolunteer()
    {
        var volunteerId = VolunteerId.NewGuid();
        var fullName = FullName.Create("Test", "Test", "Test").Value;
        var email = Email.Create("test@gmail.com").Value;
        var volunteerDescription = VolunteerDescription.Create("Test").Value;
        var workExperience = WorkExperience.Create(20).Value;
        var phoneNumber = PhoneNumber.Create("+12345678910").Value;
        var relationId = Relation.Create(Guid.NewGuid()).Value;
        var requisites = new ValueObjectList<Requisite>([Requisite.Create("Test", "Test").Value]);

        var volunteer = new Volunteer(
            volunteerId,
            fullName,
            email,
            volunteerDescription,
            workExperience,
            phoneNumber,
            relationId,
            requisites);

        return volunteer;
    }

    [Fact]
    public void Add_Issue_With_Other_Issues_Return_Success_Result()
    {
        // arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        const int petCount = 5;

        var volunteer = InitVolunteer();

        var pets = Enumerable.Range(1, petCount).Select(_ =>
            InitPet(birthDate, creationTime));

        var petToAdd = InitPet(birthDate, creationTime);

        foreach (var pet in pets)
            volunteer.AddPet(pet);

        // act
        var result = volunteer.AddPet(petToAdd);

        // assert
        var addedPetResult = volunteer.GetPetById(petToAdd.Id);

        var position = Position.Create(petCount + 1).Value;

        result.IsSuccess.Should().BeTrue();
        addedPetResult.IsSuccess.Should().BeTrue();
        addedPetResult.Value.Id.Should().Be(petToAdd.Id);
        addedPetResult.Value.Position.Should().Be(position);
    }

    [Fact]
    public void Move_Pet_Should_Not_Move_When_Position_Already_At_New_Position()
    {
        //arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        int petsCount = 5;

        var volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        var secondPosition = Position.Create(2).Value;

        var firstPet = volunteer.Pets[0];
        var secondPet = volunteer.Pets[1];
        var thirdPet = volunteer.Pets[2];
        var fourthPet = volunteer.Pets[3];
        var fifthPet = volunteer.Pets[4];


        //act
        var result = volunteer.MovePet(secondPet, secondPosition);


        //assert
        result.IsSuccess.Should().BeTrue();

        firstPet.Position.Value.Should().Be(1);
        secondPet.Position.Value.Should().Be(2);
        thirdPet.Position.Value.Should().Be(3);
        fourthPet.Position.Value.Should().Be(4);
        fifthPet.Position.Value.Should().Be(5);
    }

    [Fact]
    public void Move_Pet_Should_Move_Other_Pet_Forward_When_New_Position_Is_Lower()
    {
        //arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        int petsCount = 5;

        var volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        var secondPosition = Position.Create(2).Value;

        var firstPet = volunteer.Pets[0];
        var secondPet = volunteer.Pets[1];
        var thirdPet = volunteer.Pets[2];
        var fourthPet = volunteer.Pets[3];
        var fifthPet = volunteer.Pets[4];


        //act
        var result = volunteer.MovePet(fourthPet, secondPosition);


        //assert
        result.IsSuccess.Should().BeTrue();

        firstPet.Position.Value.Should().Be(1);
        secondPet.Position.Value.Should().Be(3);
        thirdPet.Position.Value.Should().Be(4);
        fourthPet.Position.Value.Should().Be(2);
        fifthPet.Position.Value.Should().Be(5);
    }

    [Fact]
    public void Move_Pet_Should__Move_Other_Pet_Back_When_New_Position_Is_Grater()
    {
        //arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        int petsCount = 5;

        var volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        var fourth = Position.Create(4).Value;

        var firstPet = volunteer.Pets[0];
        var secondPet = volunteer.Pets[1];
        var thirdPet = volunteer.Pets[2];
        var fourthPet = volunteer.Pets[3];
        var fifthPet = volunteer.Pets[4];


        //act
        var result = volunteer.MovePet(secondPet, fourth);


        //assert
        result.IsSuccess.Should().BeTrue();

        firstPet.Position.Value.Should().Be(1);
        secondPet.Position.Value.Should().Be(4);
        thirdPet.Position.Value.Should().Be(2);
        fourthPet.Position.Value.Should().Be(3);
        fifthPet.Position.Value.Should().Be(5);
    }

    [Fact]
    public void Move_Pet_Should__Move_Other_Pet_Forward_When_New_Position_Is_First()
    {
        //arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        int petsCount = 5;

        var volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        var first = Position.Create(1).Value;

        var firstPet = volunteer.Pets[0];
        var secondPet = volunteer.Pets[1];
        var thirdPet = volunteer.Pets[2];
        var fourthPet = volunteer.Pets[3];
        var fifthPet = volunteer.Pets[4];


        //act
        var result = volunteer.MovePet(fourthPet, first);


        //assert
        result.IsSuccess.Should().BeTrue();

        firstPet.Position.Value.Should().Be(2);
        secondPet.Position.Value.Should().Be(3);
        thirdPet.Position.Value.Should().Be(4);
        fourthPet.Position.Value.Should().Be(1);
        fifthPet.Position.Value.Should().Be(5);
    }

    [Fact]
    public void Move_Pet_Should__Move_Other_Pet_Back_When_New_Position_Is_Last()
    {
        //arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        int petsCount = 5;

        var volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        var fifth = Position.Create(5).Value;

        var firstPet = volunteer.Pets[0];
        var secondPet = volunteer.Pets[1];
        var thirdPet = volunteer.Pets[2];
        var fourthPet = volunteer.Pets[3];
        var fifthPet = volunteer.Pets[4];


        //act
        var result = volunteer.MovePet(secondPet, fifth);


        //assert
        result.IsSuccess.Should().BeTrue();

        firstPet.Position.Value.Should().Be(1);
        secondPet.Position.Value.Should().Be(5);
        thirdPet.Position.Value.Should().Be(2);
        fourthPet.Position.Value.Should().Be(3);
        fifthPet.Position.Value.Should().Be(4);
    }

    [Fact]
    public void Move_Pet_Move_Out_Of_Range_Grater_Should_Be_Error()
    {
        //arrange
        var birthDate = DateOnly.FromDateTime(DateTime.Now);
        var creationTime = DateTime.Now;
        int petsCount = 5;

        var volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        var sixth = Position.Create(7).Value;

        var firstPet = volunteer.Pets[0];
        var secondPet = volunteer.Pets[1];
        var thirdPet = volunteer.Pets[2];
        var fourthPet = volunteer.Pets[3];
        var fifthPet = volunteer.Pets[4];


        //act
        var result = volunteer.MovePet(secondPet, sixth);


        //assert
        result.IsSuccess.Should().BeFalse();
    }
}
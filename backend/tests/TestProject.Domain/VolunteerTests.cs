using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate;
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
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;

        Volunteer volunteer = InitVolunteer();

        Pet pet = InitPet(birthDate, creationTime);

        // act
        Result result = volunteer.AddPet(pet);

        // assert
        Result<Pet> addedPetResult = volunteer.GetPetById(pet.Id);

        result.IsSuccess.Should().BeTrue();
        addedPetResult.IsSuccess.Should().BeTrue();
        addedPetResult.Value.Id.Should().Be(pet.Id);
        addedPetResult.Value.Position.Should().Be(Position.First);
    }

    private static Pet InitPet(DateOnly birthDate, DateTime creationTime)
    {
        PetId petId = PetId.NewGuid();
        Name name = Name.Create("Test").Value;
        PetPhysicCharacteristics petPhysicCharacteristic = PetPhysicCharacteristics.Create(
            "Test",
            "Test",
            1,
            1,
            false,
            false).Value;
        PetDetails petDetails = PetDetails.Create("Test", birthDate, creationTime).Value;
        Address address = Address.Create(
            "Test",
            "Test",
            "Test",
            "Test").Value;
        PhoneNumber phoneNumber = PhoneNumber.Create("+12345678910").Value;
        HelpStatus helpStatus = HelpStatus.NeedsHelp;
        AnimalType animalType = new(SpeciesId.Empty(), Guid.Empty);
        ValueObjectList<Requisite> requisites = new([Requisite.Create("Test", "Test").Value]);

        Pet pet = new(
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

    private static Volunteer AddPetsInVolunteer(Volunteer volunteer, int petsCount, DateOnly birthDate,
        DateTime creationTime)
    {
        for (int i = 0; i < petsCount; i++)
        {
            Pet pet = InitPet(birthDate, creationTime);
            volunteer.AddPet(pet);
        }

        return volunteer;
    }

    private static Volunteer InitVolunteer()
    {
        VolunteerId volunteerId = VolunteerId.NewGuid();
        FullName fullName = FullName.Create("Test", "Test", "Test").Value;
        Email email = Email.Create("test@gmail.com").Value;
        VolunteerDescription volunteerDescription = VolunteerDescription.Create("Test").Value;
        WorkExperience workExperience = WorkExperience.Create(20).Value;
        PhoneNumber phoneNumber = PhoneNumber.Create("+12345678910").Value;
        ValueObjectList<Requisite> requisites = new([Requisite.Create("Test", "Test").Value]);

        Volunteer volunteer = new(
            volunteerId,
            fullName,
            email,
            volunteerDescription,
            workExperience,
            phoneNumber,
            requisites);

        return volunteer;
    }

    [Fact]
    public void Add_Issue_With_Other_Issues_Return_Success_Result()
    {
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        const int petCount = 5;

        Volunteer volunteer = InitVolunteer();

        IEnumerable<Pet> pets = Enumerable.Range(1, petCount).Select(_ =>
            InitPet(birthDate, creationTime));

        Pet petToAdd = InitPet(birthDate, creationTime);

        foreach (Pet pet in pets)
        {
            volunteer.AddPet(pet);
        }

        // act
        Result result = volunteer.AddPet(petToAdd);

        // assert
        Result<Pet> addedPetResult = volunteer.GetPetById(petToAdd.Id);

        Position position = Position.Create(petCount + 1).Value;

        result.IsSuccess.Should().BeTrue();
        addedPetResult.IsSuccess.Should().BeTrue();
        addedPetResult.Value.Id.Should().Be(petToAdd.Id);
        addedPetResult.Value.Position.Should().Be(position);
    }

    [Fact]
    public void Move_Pet_Should_Not_Move_When_Position_Already_At_New_Position()
    {
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        int petsCount = 5;

        Volunteer volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        Position secondPosition = Position.Create(2).Value;

        Pet firstPet = volunteer.Pets[0];
        Pet secondPet = volunteer.Pets[1];
        Pet thirdPet = volunteer.Pets[2];
        Pet fourthPet = volunteer.Pets[3];
        Pet fifthPet = volunteer.Pets[4];

        // act
        Result result = volunteer.MovePet(secondPet, secondPosition);

        // assert
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
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        int petsCount = 5;

        Volunteer volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        Position secondPosition = Position.Create(2).Value;

        Pet firstPet = volunteer.Pets[0];
        Pet secondPet = volunteer.Pets[1];
        Pet thirdPet = volunteer.Pets[2];
        Pet fourthPet = volunteer.Pets[3];
        Pet fifthPet = volunteer.Pets[4];

        // act
        Result result = volunteer.MovePet(fourthPet, secondPosition);

        // assert
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
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        int petsCount = 5;

        Volunteer volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        Position fourth = Position.Create(4).Value;

        Pet firstPet = volunteer.Pets[0];
        Pet secondPet = volunteer.Pets[1];
        Pet thirdPet = volunteer.Pets[2];
        Pet fourthPet = volunteer.Pets[3];
        Pet fifthPet = volunteer.Pets[4];

        // act
        Result result = volunteer.MovePet(secondPet, fourth);

        // assert
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
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        int petsCount = 5;

        Volunteer volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        Position first = Position.Create(1).Value;

        Pet firstPet = volunteer.Pets[0];
        Pet secondPet = volunteer.Pets[1];
        Pet thirdPet = volunteer.Pets[2];
        Pet fourthPet = volunteer.Pets[3];
        Pet fifthPet = volunteer.Pets[4];

        // act
        Result result = volunteer.MovePet(fourthPet, first);

        // assert
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
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        int petsCount = 5;

        Volunteer volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        Position fifth = Position.Create(5).Value;

        Pet firstPet = volunteer.Pets[0];
        Pet secondPet = volunteer.Pets[1];
        Pet thirdPet = volunteer.Pets[2];
        Pet fourthPet = volunteer.Pets[3];
        Pet fifthPet = volunteer.Pets[4];

        // act
        Result result = volunteer.MovePet(secondPet, fifth);

        // assert
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
        // arrange
        DateOnly birthDate = DateOnly.FromDateTime(DateTime.Now);
        DateTime creationTime = DateTime.Now;
        int petsCount = 5;

        Volunteer volunteer = InitVolunteer();
        volunteer = AddPetsInVolunteer(volunteer, petsCount, birthDate, creationTime);

        Position sixth = Position.Create(7).Value;

        _ = volunteer.Pets[0];
        Pet secondPet = volunteer.Pets[1];

        _ = volunteer.Pets[2];

        _ = volunteer.Pets[3];

        _ = volunteer.Pets[4];

        // act
        Result result = volunteer.MovePet(secondPet, sixth);

        // assert
        result.IsSuccess.Should().BeFalse();
    }
}
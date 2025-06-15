using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate;

public class Volunteer : Entity<VolunteerId>, ISoftDeletable
{
    private readonly List<Pet> _pets = [];
    private List<Requisite> _requisites = [];
    private List<Skill> _skills = [];

    //Ef core configuration
    private Volunteer(VolunteerId id) : base(id)
    {
    }

    public Volunteer(
        VolunteerId volunteerId,
        FullName fullName,
        Email email,
        VolunteerDescription volunteerDescription,
        WorkExperience workExperience,
        PhoneNumber phone,
        Relation relation,
        ValueObjectList<Requisite> requisites,
        ValueObjectList<Skill> skills)
        : base(volunteerId)
    {
        FullName = fullName;
        Email = email;
        Description = volunteerDescription;
        WorkExperience = workExperience;
        Phone = phone;
        Relation = relation;
        _requisites = requisites;
        _skills = skills;
    }

    public FullName FullName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public VolunteerDescription Description { get; private set; }
    public WorkExperience WorkExperience { get; private set; }
    public Relation Relation { get; private set; }
    public IReadOnlyList<Requisite> Requisites => _requisites;
    public IReadOnlyList<Skill> Skills => _skills;
    public IReadOnlyList<Pet> Pets => _pets;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletionDate { get; private set; }

    public Result AddPet(Pet pet)
    {
        var position = _pets.Count == 0
            ? Position.First
            : Position.Create(_pets.Count + 1);

        if (position.IsFailure)
            return position.Errors;

        pet.SetPosition(position.Value);
        _pets.Add(pet);
        return Result.Success();
    }

    public Result SetMainPhotoOfPet(PetId petId, PetPhoto petPhoto)
    {
        var pet = GetPetById(petId);
        if (pet.IsFailure)
            return pet.Errors;

        var result = pet.Value.SetMainPhoto(petPhoto);
        if (result.IsFailure)
            return result.Errors;

        return result;
    }

    public Result UpdateSkills(List<Skill> skills)
    {
        _skills = skills;
        return Result.Success();
    }
    
    public Result UpdatePet(
        PetId petId,
        Name? name,
        PetPhysicCharacteristics? petPhysicCharacteristics,
        PetDetails? petDetails,
        Address? address,
        PhoneNumber? phoneNumber,
        HelpStatus? helpStatus,
        AnimalType? animalType,
        AnimalSex? animalSex,
        History? history,
        Temperament? temperament,
        MedicalInfo? medicalInfo,
        ValueObjectList<Requisite>? requisites)
    {
        var pet = GetPetById(petId);
        if (pet.IsFailure)
            return Errors.General.NotFound();

        pet.Value.UpdatePet(
            name,
            petPhysicCharacteristics,
            petDetails,
            address,
            phoneNumber,
            helpStatus,
            animalType,
            animalSex,
            history,
            temperament,
            medicalInfo,
            requisites);

        return Result.Success();
    }

    public Result UpdatePetStatus(PetId petId, HelpStatus helpStatus)
    {
        var pet = GetPetById(petId);
        if (pet.IsFailure)
            return pet.Errors;

        pet.Value.UpdateHelpStatus(helpStatus);
        return Result.Success();
    }


    public Result DeletePetSoft(PetId petId, DateTime deletionDate)
    {
        var pet = GetPetById(petId);
        if (pet.IsFailure)
            return pet.Errors;

        RecalculatePositionOfOtherPets(pet.Value.Position);

        pet.Value.Delete();
        return Result.Success();
    }

    public Result DeletePetForce(PetId petId, DateTime deletionDate)
    {
        var pet = GetPetById(petId);
        if (pet.IsFailure)
            return pet.Errors;

        RecalculatePositionOfOtherPets(pet.Value.Position);

        _pets.Remove(pet.Value);
        pet.Value.Delete();
        return Result.Success();
    }

    public void DeleteExpiredPets(int expiredTime)
    {
        _pets.RemoveAll(p => p.DeletionDate != null
                             && p.DeletionDate.Value.AddDays(expiredTime) <= DateTime.UtcNow);
    }

    public Result MovePet(Pet pet, Position newPosition)
    {
        var currentPosition = pet.Position;

        if (currentPosition == newPosition || _pets.Count == 1)
            return Result.Success();

        var adjustedPosition = IfNewPositionOutOfRange(newPosition);
        if (adjustedPosition.IsFailure)
            return adjustedPosition.Errors;

        newPosition = adjustedPosition.Value;

        var moveResult = MovePetBetweenPositions(newPosition, currentPosition);
        if (moveResult.IsFailure)
            return moveResult.Errors;

        pet.Move(newPosition);
        return Result.Success();
    }

    private Result MovePetBetweenPositions(Position newPosition, Position currentPosition)
    {
        if (newPosition.Value < currentPosition.Value)
        {
            var petsToMove = _pets.Where(p => p.Position.Value >= newPosition.Value
                                              && p.Position.Value < currentPosition.Value);

            foreach (var petToMove in petsToMove)
            {
                var result = petToMove.MoveForward();
                if (result.IsFailure)
                {
                    return result.Errors;
                }
            }
        }
        else if (newPosition.Value > currentPosition.Value)
        {
            var petsToMove = _pets.Where(p => p.Position.Value <= newPosition.Value
                                              && p.Position.Value > currentPosition.Value);

            foreach (var petToMove in petsToMove)
            {
                var result = petToMove.MoveBack();
                if (result.IsFailure)
                {
                    return result.Errors;
                }
            }
        }

        return Result.Success();
    }

    private Result<Position> IfNewPositionOutOfRange(Position newPosition)
    {
        if (newPosition.Value > _pets.Count)
            return Errors.Volunteer.PetPositionOutOfRange();

        return newPosition;
    }

    public int PetsNeedsHelp() => _pets.Count(x => x.HelpStatus == HelpStatus.NeedsHelp);
    public int PetsSearchingHome() => _pets.Count(x => x.HelpStatus == HelpStatus.SearchingHome);
    public int PetsFoundHome() => _pets.Count(x => x.HelpStatus == HelpStatus.FoundHome);

    public Result UpdateSocialNetworks(List<SocialNetwork> socialNetworks)
    {
        return Result.Success();
    }

    public Result UpdateRequisites(List<Requisite> requisites)
    {
        _requisites = requisites;
        return Result.Success();
    }

    public Result<Pet> GetPetById(PetId id)
    {
        var pet = _pets.FirstOrDefault(p => p.Id == id);

        if (pet == null)
            return Errors.General.NotFound(id.Id);

        return pet;
    }

    public Result UpdateInfo(
        FullName? fullName,
        PhoneNumber? phoneNumber,
        WorkExperience? workExperience)
    {
        FullName = fullName ?? FullName;
        Phone = phoneNumber ?? Phone;
        WorkExperience = workExperience ?? WorkExperience;

        return Result.Success();
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletionDate = DateTime.UtcNow;

        _pets.ForEach(p => p.Delete());
    }

    private Result RecalculatePositionOfOtherPets(Position currentPosition)
    {
        if (currentPosition.Value == _pets.Count)
            return Result.Success();

        var petsToMove = _pets.Where(p => p.Position.Value > currentPosition.Value);
        foreach (var pet in petsToMove)
        {
            var result = pet.MoveBack();
            if (result.IsFailure)
                return result.Errors;
        }

        return Result.Success();
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletionDate = null;
        _pets.ForEach(p => p.Restore());
    }

    public Result RestorePet(PetId id)
    {
        var pet = GetPetById(id);
        if (pet.IsFailure)
            return pet.Errors;

        pet.Value.Restore();

        var resultMove = MovePet(pet.Value, Position.Create(_pets.Count).Value);
        if (resultMove.IsFailure)
            return resultMove.Errors;

        return Result.Success();
    }
}
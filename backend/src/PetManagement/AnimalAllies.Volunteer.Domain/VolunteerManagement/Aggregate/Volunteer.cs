using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate;

public class Volunteer : Entity<VolunteerId>, ISoftDeletable
{
    private readonly List<Pet> _pets = [];
    private List<Requisite> _requisites = [];

    // Ef core configuration
    private Volunteer(VolunteerId id)
        : base(id)
    {
    }

    public Volunteer(
        VolunteerId volunteerId,
        FullName fullName,
        Email email,
        VolunteerDescription volunteerDescription,
        WorkExperience workExperience,
        PhoneNumber phone,
        ValueObjectList<Requisite> requisites)
        : base(volunteerId)
    {
        FullName = fullName;
        Email = email;
        Description = volunteerDescription;
        WorkExperience = workExperience;
        Phone = phone;
        _requisites = requisites;
    }

    public FullName FullName { get; private set; }

    public Email Email { get; private set; }

    public PhoneNumber Phone { get; private set; }

    public VolunteerDescription Description { get; private set; }

    public WorkExperience WorkExperience { get; private set; }

    public IReadOnlyList<Requisite> Requisites => _requisites;

    public IReadOnlyList<Pet> Pets => _pets;

    public bool IsDeleted { get; private set; }

    public DateTime? DeletionDate { get; private set; }

    public void Delete()
    {
        IsDeleted = true;
        DeletionDate = DateTime.UtcNow;

        _pets.ForEach(p => p.Delete());
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletionDate = null;
        _pets.ForEach(p => p.Restore());
    }

    public Result AddPet(Pet pet)
    {
        Result<Position> position = _pets.Count == 0
            ? Position.First
            : Position.Create(_pets.Count + 1);

        if (position.IsFailure)
        {
            return position.Errors;
        }

        pet.SetPosition(position.Value);
        _pets.Add(pet);
        return Result.Success();
    }

    public Result SetMainPhotoOfPet(PetId petId, PetPhoto petPhoto)
    {
        Result<Pet> pet = GetPetById(petId);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        Result result = pet.Value.SetMainPhoto(petPhoto);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        return result;
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
        ValueObjectList<Requisite>? requisites)
    {
        Result<Pet> pet = GetPetById(petId);
        if (pet.IsFailure)
        {
            return Errors.General.NotFound();
        }

        pet.Value.UpdatePet(
            name,
            petPhysicCharacteristics,
            petDetails,
            address,
            phoneNumber,
            helpStatus,
            animalType,
            requisites);

        return Result.Success();
    }

    public Result UpdatePetStatus(PetId petId, HelpStatus helpStatus)
    {
        Result<Pet> pet = GetPetById(petId);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        pet.Value.UpdateHelpStatus(helpStatus);
        return Result.Success();
    }

    public Result DeletePetSoft(PetId petId, DateTime deletionDate)
    {
        Result<Pet> pet = GetPetById(petId);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        RecalculatePositionOfOtherPets(pet.Value.Position);

        pet.Value.Delete();
        return Result.Success();
    }

    public Result DeletePetForce(PetId petId, DateTime deletionDate)
    {
        Result<Pet> pet = GetPetById(petId);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        RecalculatePositionOfOtherPets(pet.Value.Position);

        _pets.Remove(pet.Value);
        pet.Value.Delete();
        return Result.Success();
    }

    public void DeleteExpiredPets(int expiredTime) =>
        _pets.RemoveAll(p => p.DeletionDate != null
                             && p.DeletionDate.Value.AddDays(expiredTime) <= DateTime.UtcNow);

    public Result MovePet(Pet pet, Position newPosition)
    {
        Position currentPosition = pet.Position;

        if (currentPosition == newPosition || _pets.Count == 1)
        {
            return Result.Success();
        }

        Result<Position> adjustedPosition = IfNewPositionOutOfRange(newPosition);
        if (adjustedPosition.IsFailure)
        {
            return adjustedPosition.Errors;
        }

        newPosition = adjustedPosition.Value;

        Result moveResult = MovePetBetweenPositions(newPosition, currentPosition);
        if (moveResult.IsFailure)
        {
            return moveResult.Errors;
        }

        pet.Move(newPosition);
        return Result.Success();
    }

    private Result MovePetBetweenPositions(Position newPosition, Position currentPosition)
    {
        if (newPosition.Value < currentPosition.Value)
        {
            IEnumerable<Pet> petsToMove = _pets.Where(p => p.Position.Value >= newPosition.Value
                                                           && p.Position.Value < currentPosition.Value);

            foreach (Pet petToMove in petsToMove)
            {
                Result result = petToMove.MoveForward();
                if (result.IsFailure)
                {
                    return result.Errors;
                }
            }
        }
        else if (newPosition.Value > currentPosition.Value)
        {
            IEnumerable<Pet> petsToMove = _pets.Where(p => p.Position.Value <= newPosition.Value
                                                           && p.Position.Value > currentPosition.Value);

            foreach (Pet petToMove in petsToMove)
            {
                Result result = petToMove.MoveBack();
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
        {
            return Errors.Volunteer.PetPositionOutOfRange();
        }

        return newPosition;
    }

    public int PetsNeedsHelp() => _pets.Count(x => x.HelpStatus == HelpStatus.NeedsHelp);

    public int PetsSearchingHome() => _pets.Count(x => x.HelpStatus == HelpStatus.SearchingHome);

    public int PetsFoundHome() => _pets.Count(x => x.HelpStatus == HelpStatus.FoundHome);

    public static Result UpdateSocialNetworks(List<SocialNetwork> socialNetworks) => Result.Success();

    public Result UpdateRequisites(List<Requisite> requisites)
    {
        _requisites = requisites;
        return Result.Success();
    }

    public Result<Pet> GetPetById(PetId id)
    {
        Pet? pet = _pets.FirstOrDefault(p => p.Id == id);

        if (pet == null)
        {
            return Errors.General.NotFound(id.Id);
        }

        return pet;
    }

    public Result UpdateInfo(
        FullName? fullName,
        Email? email,
        PhoneNumber? phoneNumber,
        VolunteerDescription? description,
        WorkExperience? workExperience)
    {
        FullName = fullName ?? FullName;
        Email = email ?? Email;
        Phone = phoneNumber ?? Phone;
        Description = description ?? Description;
        WorkExperience = workExperience ?? WorkExperience;

        return Result.Success();
    }

    private Result RecalculatePositionOfOtherPets(Position currentPosition)
    {
        if (currentPosition.Value == _pets.Count)
        {
            return Result.Success();
        }

        IEnumerable<Pet> petsToMove = _pets.Where(p => p.Position.Value > currentPosition.Value);
        foreach (Pet pet in petsToMove)
        {
            Result result = pet.MoveBack();
            if (result.IsFailure)
            {
                return result.Errors;
            }
        }

        return Result.Success();
    }

    public Result RestorePet(PetId id)
    {
        Result<Pet> pet = GetPetById(id);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        pet.Value.Restore();

        Result resultMove = MovePet(pet.Value, Position.Create(_pets.Count).Value);
        if (resultMove.IsFailure)
        {
            return resultMove.Errors;
        }

        return Result.Success();
    }
}
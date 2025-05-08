using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;

public class Pet : Entity<PetId>, ISoftDeletable
{
    private Pet(PetId id)
        : base(id)
    {
    }

    public Pet(
        PetId petId,
        Name name,
        PetPhysicCharacteristics petPhysicCharacteristics,
        PetDetails petDetails,
        Address address,
        PhoneNumber phoneNumber,
        HelpStatus helpStatus,
        AnimalType animalType,
        ValueObjectList<Requisite> requisites)
        : base(petId)
    {
        Name = name;
        PetPhysicCharacteristics = petPhysicCharacteristics;
        PetDetails = petDetails;
        Address = address;
        PhoneNumber = phoneNumber;
        HelpStatus = helpStatus;
        AnimalType = animalType;
        Requisites = requisites;
    }

    public Name Name { get; private set; }

    public PetPhysicCharacteristics PetPhysicCharacteristics { get; private set; }

    public PetDetails PetDetails { get; private set; }

    public Address Address { get; private set; }

    public PhoneNumber PhoneNumber { get; private set; }

    public HelpStatus HelpStatus { get; private set; }

    public AnimalType AnimalType { get; private set; }

    public Position Position { get; private set; }

    public IReadOnlyList<Requisite> Requisites { get; private set; }

    public IReadOnlyList<PetPhoto> PetPhotoDetails { get; private set; } = [];

    public bool IsDeleted { get; private set; }

    public DateTime? DeletionDate { get; private set; }

    public void Delete()
    {
        IsDeleted = true;
        DeletionDate = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletionDate = null;
    }

    public Result AddPhotos(ValueObjectList<PetPhoto>? photos)
    {
        if (photos is null)
        {
            return Errors.General.Null("photos");
        }

        IEnumerable<PetPhoto> newPhotoList = PetPhotoDetails.Union(photos);

        PetPhotoDetails = [.. newPhotoList];

        return Result.Success();
    }

    public Result DeletePhotos(IEnumerable<FilePath> filePaths)
    {
        IEnumerable<PetPhoto> photos = PetPhotoDetails.Where(f => !filePaths.Contains(f.Path));

        PetPhotoDetails = [.. photos];

        return Result.Success();
    }

    public void SetPosition(Position position) => Position = position;

    public Result MoveForward()
    {
        Result<Position> newPosition = Position.Forward();
        if (newPosition.IsFailure)
        {
            return newPosition.Errors;
        }

        Position = newPosition.Value;

        return Result.Success();
    }

    public Result SetMainPhoto(PetPhoto petPhoto)
    {
        PetPhoto? isPhotoExist = PetPhotoDetails.FirstOrDefault(p => p.Path == petPhoto.Path);
        if (isPhotoExist is null)
        {
            return Errors.General.NotFound();
        }

        PetPhotoDetails =
        [
            .. PetPhotoDetails
                .Select(photo => new PetPhoto(photo.Path, photo.Path == petPhoto.Path))
                .OrderByDescending(p => p.IsMain)
        ];

        return Result.Success();
    }

    public Result MoveBack()
    {
        Result<Position> newPosition = Position.Back();
        if (newPosition.IsFailure)
        {
            return newPosition.Errors;
        }

        Position = newPosition.Value;

        return Result.Success();
    }

    public Result UpdateHelpStatus(HelpStatus helpStatus)
    {
        HelpStatus = helpStatus;

        return Result.Success();
    }

    public void Move(Position newPosition) => Position = newPosition;

    public void UpdatePet(
        Name? name,
        PetPhysicCharacteristics? petPhysicCharacteristics,
        PetDetails? petDetails,
        Address? address,
        PhoneNumber? phoneNumber,
        HelpStatus? helpStatus,
        AnimalType? animalType,
        ValueObjectList<Requisite>? requisites)
    {
        Name = name ?? Name;
        PetPhysicCharacteristics = petPhysicCharacteristics ?? PetPhysicCharacteristics;
        PetDetails = petDetails ?? PetDetails;
        Address = address ?? Address;
        PhoneNumber = phoneNumber ?? PhoneNumber;
        HelpStatus = helpStatus ?? HelpStatus;
        AnimalType = animalType ?? AnimalType;
        Requisites = requisites ?? Requisites;
    }
}
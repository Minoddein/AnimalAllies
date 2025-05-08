using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class FilePath : ValueObject
{
    private FilePath() { }

    private FilePath(string path) => Path = path;

    public string Path { get; }

    public static Result<FilePath> Create(Guid path, string extension)
    {
        if (!Constraints.Extensions.Contains(extension))
        {
            return Errors.General.ValueIsInvalid("extension");
        }

        if (string.IsNullOrWhiteSpace(extension) || extension.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsRequired(extension);
        }

        string fullPath = path + extension;

        return new FilePath(fullPath);
    }

    public static Result<FilePath> Create(string fullPath) => new FilePath(fullPath);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Path;
    }
}
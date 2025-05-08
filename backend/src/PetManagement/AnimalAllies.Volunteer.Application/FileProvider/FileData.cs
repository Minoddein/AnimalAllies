using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

namespace AnimalAllies.Volunteer.Application.FileProvider;

public record FileData(Stream Stream, FileInfo FileInfo);

public record FileInfo(FilePath FilePath, string BucketName);
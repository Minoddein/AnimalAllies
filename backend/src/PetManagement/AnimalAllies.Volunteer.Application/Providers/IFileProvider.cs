using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Volunteer.Application.FileProvider;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FileInfo = AnimalAllies.Volunteer.Application.FileProvider.FileInfo;

namespace AnimalAllies.Volunteer.Application.Providers;

public interface IFileProvider
{
    Task<Result<IReadOnlyList<FilePath>>> UploadFiles(
        IEnumerable<FileData> filesData,
        CancellationToken cancellationToken);

    Task<Result<string>> DeleteFile(FileMetadata fileMetadata, CancellationToken cancellationToken);

    Task<Result<string>> GetFileByObjectName(FileMetadata fileMetadata, CancellationToken cancellationToken);

    Task<Result> RemoveFile(FileInfo fileInfo, CancellationToken cancellationToken);
}
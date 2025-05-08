using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.Volunteer.Application.FileProvider;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel;
using Minio.DataModel.Args;
using FileInfo = AnimalAllies.Volunteer.Application.FileProvider.FileInfo;
using IFileProvider = AnimalAllies.Volunteer.Application.Providers.IFileProvider;

namespace AnimalAllies.Volunteer.Infrastructure.Providers;

public class MinioProvider(IMinioClient minioClient, ILogger<MinioProvider> logger) : IFileProvider
{
    private const int MAX_DEGREE_OF_PARALLELISM = 10;
    private readonly ILogger<MinioProvider> _logger = logger;
    private readonly IMinioClient _minioClient = minioClient;

    public async Task<Result<IReadOnlyList<FilePath>>> UploadFiles(
        IEnumerable<FileData> filesData,
        CancellationToken cancellationToken = default)
    {
        SemaphoreSlim semaphoreSlim = new(MAX_DEGREE_OF_PARALLELISM);
        List<FileData> filesList = [.. filesData];

        try
        {
            await IsBucketExist(filesList.Select(f => f.FileInfo.BucketName), cancellationToken).ConfigureAwait(false);

            IEnumerable<Task<Result<FilePath>>> tasks = filesList.Select(async file =>
                await PutObject(file, semaphoreSlim, cancellationToken).ConfigureAwait(false));

            Result<FilePath>[] pathsResult = await Task.WhenAll(tasks).ConfigureAwait(false);

            if (pathsResult.Any(p => p.IsFailure))
            {
                return pathsResult.First().Errors;
            }

            List<FilePath> results = [.. pathsResult.Select(p => p.Value)];

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Fail to upload files in minio, files amount: {amount}", filesList.Count);

            return Error.Failure("file.upload", "Fail to upload files in minio");
        }
    }

    public async Task<Result<string>> DeleteFile(FileMetadata fileMetadata, CancellationToken cancellationToken)
    {
        try
        {
            PresignedGetObjectArgs? objectExistArgs = new PresignedGetObjectArgs()
                .WithBucket(fileMetadata.BucketName)
                .WithObject(fileMetadata.ObjectName)
                .WithExpiry(60 * 60 * 24);

            string? objectExist = await _minioClient.PresignedGetObjectAsync(objectExistArgs).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(objectExist))
            {
                return Error.NotFound("object.not.found", "File doesn`t exist in minio");
            }

            RemoveObjectArgs? removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(fileMetadata.BucketName)
                .WithObject(fileMetadata.ObjectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken).ConfigureAwait(false);

            return fileMetadata.ObjectName;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to delete file in minio");
            return Error.Failure("file.delete", "Fail to delete file in minio");
        }
    }

    public async Task<Result> RemoveFile(
        FileInfo fileInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await IsBucketExist([fileInfo.BucketName], cancellationToken).ConfigureAwait(false);

            StatObjectArgs? statArgs = new StatObjectArgs()
                .WithBucket(fileInfo.BucketName)
                .WithObject(fileInfo.FilePath.Path);

            RemoveObjectArgs? arg = new RemoveObjectArgs()
                .WithBucket(fileInfo.BucketName)
                .WithObject(fileInfo.FilePath.Path);

            await _minioClient.RemoveObjectAsync(arg, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to remove file in minio with path {path} in bucket {bucket}",
                fileInfo.FilePath.Path, fileInfo.BucketName);
            return Error.Failure("file.delete", "Fail to delete file in minio");
        }

        return Result.Success();
    }

    public async Task<Result<string>> GetFileByObjectName(
        FileMetadata fileMetadata,
        CancellationToken cancellationToken)
    {
        try
        {
            StatObjectArgs? objectExistArgs = new StatObjectArgs()
                .WithBucket(fileMetadata.BucketName)
                .WithObject(fileMetadata.ObjectName);

            ObjectStat? objectStat = await _minioClient.StatObjectAsync(objectExistArgs, cancellationToken)
                .ConfigureAwait(false);

            if (objectStat == null)
            {
                return Error.NotFound("object.not.found", "File doesn`t exist in minio");
            }

            return objectStat.ObjectName;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to get file in minio");
            return Error.Failure("file.get", "Fail to get file in minio");
        }
    }

    [Obsolete]
    public Result<IReadOnlyCollection<string>> GetFiles()
    {
        ListObjectsArgs? listObjectsArgs = new ListObjectsArgs()
            .WithBucket("photos")
            .WithRecursive(false);

        IObservable<Item>? objects = _minioClient.ListObjectsAsync(listObjectsArgs);

        List<string> paths = [];

        IDisposable subscription = objects.Subscribe(
            item => paths.Add(item.Key),
            ex => _logger.LogError(ex, "Failed to get files"),
            () => _logger.LogInformation("Successfully uploaded files"));

        return paths;
    }

    private async Task<Result<FilePath>> PutObject(
        FileData fileData,
        SemaphoreSlim semaphoreSlim,
        CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        PutObjectArgs? putObjectArgs = new PutObjectArgs()
            .WithBucket(fileData.FileInfo.BucketName)
            .WithStreamData(fileData.Stream)
            .WithObjectSize(fileData.Stream.Length)
            .WithObject(fileData.FileInfo.FilePath.Path);

        try
        {
            await _minioClient
                .PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

            return fileData.FileInfo.FilePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Fail to upload file in minio with path {path} in bucket {bucket}",
                fileData.FileInfo.FilePath.Path,
                fileData.FileInfo.BucketName);

            return Error.Failure("file.upload", "Fail to upload file in minio");
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private async Task IsBucketExist(IEnumerable<string> bucketNames, CancellationToken cancellationToken)
    {
        HashSet<string> buckets = [.. bucketNames];

        foreach (string bucketName in buckets)
        {
            BucketExistsArgs? bucketExistArgs = new BucketExistsArgs()
                .WithBucket(bucketName);

            bool bucketExist = await _minioClient.BucketExistsAsync(bucketExistArgs, cancellationToken)
                .ConfigureAwait(false);

            if (bucketExist == false)
            {
                MakeBucketArgs? makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);

                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
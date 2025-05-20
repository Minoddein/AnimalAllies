using System.Collections.Concurrent;
using Amazon.S3;
using Amazon.S3.Model;
using FileService.Application.Providers;
using FileService.Data.Models;
using FileService.Data.Shared;


namespace FileService.Infrastructure.Providers;

public class MinioProvider : IFileProvider
{
    private const int MAX_DEGREE_OF_PARALLELISM = 50;
    private const int EXPIRATION_URL = 1;
    private const int EXPIRATION_FOR_DOWNLOAD_URL = 30;
    private readonly IAmazonS3 _client;
    private readonly ILogger<MinioProvider> _logger;

    public MinioProvider(IAmazonS3 client, ILogger<MinioProvider> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<List<string>>> GetPresignedUrlsForDeleteParallel(IEnumerable<FileMetadata> fileMetadata, CancellationToken cancellationToken)
    {
        var fileMetadatas = fileMetadata.ToList();
        
        try
        {
            var results = new ConcurrentBag<string>();
            var errors = new ConcurrentBag<ErrorList>();

            await Parallel.ForEachAsync(fileMetadatas, new ParallelOptions
                {
                    MaxDegreeOfParallelism = MAX_DEGREE_OF_PARALLELISM,
                    CancellationToken = cancellationToken
                },
                async (metadata, token) =>
                {
                    var deleteRequest = new GetPreSignedUrlRequest
                    {
                        BucketName = metadata.BucketName,
                        Key = metadata.Key,
                        Verb = HttpVerb.DELETE,
                        Expires = DateTime.UtcNow.AddDays(EXPIRATION_URL),
                        Protocol = Protocol.HTTP,
                    };

                    var result = await _client.GetPreSignedURLAsync(deleteRequest);

                    if (result is null)
                        errors.Add(Error.NotFound("object.not.found", 
                            "File does`t exist in minio"));
                    else
                        results.Add(result);
                });
            
            if (errors.Any())
                return Error.Failure("file.upload", $"Failed to upload {errors.Count} files");
    
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fail to upload files in minio");

            return Error.Failure("files.upload", "Fail to upload files in minio");
        }
    }

    public async Task DeleteFile(FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = fileMetadata.BucketName,
                Key = fileMetadata.Key
            };
            
            await _client.DeleteObjectAsync(request, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Fail to delete file in minio");
        }
    }

    public async Task<InitiateMultipartUploadResponse> InitialMultipartUpload(
        FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        var presignedRequest = new InitiateMultipartUploadRequest
        {
            BucketName = fileMetadata.BucketName,
            Key = fileMetadata.Key,
            ContentType = fileMetadata.ContentType,
            Metadata =
            {
                ["file-name"] = fileMetadata.FileName
            }
        };

        var response = await _client
            .InitiateMultipartUploadAsync(presignedRequest, cancellationToken);

        return response;
    }
    
    public async Task<Result<string>> GetPresignedUrlPartForUpload(
        FileMetadata fileMetadata,
        CancellationToken cancellationToken)
    {
        try
        {
            var presignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = fileMetadata.BucketName,
                Key = fileMetadata.Key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddDays(EXPIRATION_URL),
                Protocol = Protocol.HTTP,
                UploadId = fileMetadata.UploadId,
                PartNumber = fileMetadata.PartNumber,
            };
            
            var result = await _client.GetPreSignedURLAsync(presignedRequest);
    
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fail to upload file in minio with path {path} in bucket {bucket}",
                fileMetadata.FullPath,
                fileMetadata.BucketName);

            return Error.Failure("file.upload", "Fail to upload file in minio");
        }
    }

    public async Task<CompleteMultipartUploadResponse> CompleteMultipartUpload(
        FileMetadata fileMetadata, CancellationToken cancellationToken = default)
    {
        var presignedRequest = new CompleteMultipartUploadRequest()
        {
            BucketName = fileMetadata.BucketName,
            Key = fileMetadata.Key,
            UploadId = fileMetadata.UploadId,
            PartETags = fileMetadata.ETags!.Select(e => new PartETag(e.PartNumber, e.ETag)).ToList()
        };

        var response = await _client
            .CompleteMultipartUploadAsync(presignedRequest, cancellationToken);

        return response;
    }


    public async Task<Result<string>> GetPresignedUrlForDownload(
        FileMetadata fileMetadata, CancellationToken cancellationToken)
    {
        try
        {
            var presignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = fileMetadata.BucketName,
                Key = fileMetadata.Key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddDays(EXPIRATION_FOR_DOWNLOAD_URL),
                Protocol = Protocol.HTTP,
            };

            var url = await _client.GetPreSignedURLAsync(presignedRequest);

            if (url is null)
                return Error.NotFound("object.not.found", "File doesn`t exist in minio");

            return url;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to get file in minio");
            return Error.Failure("file.get", "Fail to get file in minio");
        }
    }

    public async Task<FileMetadata> GetObjectMetadata(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        var metaDataRequest = new GetObjectMetadataRequest
        {
            BucketName = bucketName,
            Key = key
        };
        try
        {
            var metaDataResponse = await _client.GetObjectMetadataAsync(metaDataRequest, cancellationToken);

            var metaData = new FileMetadata
            {
                Id = Guid.NewGuid(),
                Key = key,
                Size = metaDataResponse.Headers.ContentLength,
                ContentType = metaDataResponse.Headers.ContentType
            };

            return metaData;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Result<string>> GetPresignedUrlForDelete(
        FileMetadata fileMetadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await IsBucketExist([fileMetadata.BucketName], cancellationToken);
            
            var deleteRequest = new GetPreSignedUrlRequest
            {
                BucketName = fileMetadata.BucketName,
                Key = fileMetadata.Key,
                Verb = HttpVerb.DELETE,
                Expires = DateTime.UtcNow.AddDays(EXPIRATION_URL),
                Protocol = Protocol.HTTP,
            };
            
            var url = await _client.GetPreSignedURLAsync(deleteRequest);
            
            if (url is null)
                return Error.NotFound("object.not.found", "File doesn`t exist in minio");
            
            return url;
        }
        catch(Exception e)
        {
            _logger.LogError(e,"Fail to remove file in minio with path {path} in bucket {bucket}",
                fileMetadata.FullPath, fileMetadata.BucketName);
            return Error.Failure("file.delete", "Fail to delete file in minio");
        }
    }
    

    public async Task<Result<string>> GetPresignedUrlForDownload(
        FileMetadata fileMetadata,
        SemaphoreSlim semaphoreSlim,
        CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            var presignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = fileMetadata.BucketName,
                Key = fileMetadata.Key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddDays(EXPIRATION_URL),
                Protocol = Protocol.HTTP,
            };

            var url = await _client.GetPreSignedURLAsync(presignedRequest);

            if (url is null)
                return Error.NotFound("object.not.found", "File doesn`t exist in minio");

            return url;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fail to get file in minio");
            return Error.Failure("file.get", "Fail to get file in minio");
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
    
    public async Task<Result<IReadOnlyList<string>>> DownloadFiles(
        IEnumerable<FileMetadata> filesMetadata,
        CancellationToken cancellationToken = default)
    {
        var semaphoreSlim = new SemaphoreSlim(MAX_DEGREE_OF_PARALLELISM);
        var filesList = filesMetadata.ToList();

        try
        {
            await IsBucketExist(filesList.Select(f => f.BucketName), cancellationToken);

            var tasks = filesList.Select(async file =>
                await GetPresignedUrlForDownload(file, semaphoreSlim, cancellationToken));

            var pathsResult = await Task.WhenAll(tasks);

            if (pathsResult.Any(p => p.IsFailure))
                return pathsResult.First().Errors;

            var results = pathsResult.Select(p => p.Value).ToList();

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fail to download files, files amount: {amount}", filesList.Count);

            return Error.Failure("file.download", "Fail to download files");
        }
    }

    public async Task<Result<string>> GetPresignedUrlForUpload(
        FileMetadata fileMetadata,
        CancellationToken cancellationToken)
    {
        try
        {
            var presignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = fileMetadata.BucketName,
                Key = Uri.EscapeDataString(fileMetadata.Key),
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddDays(EXPIRATION_URL),
                ContentType = fileMetadata.ContentType,
                Protocol = Protocol.HTTP,
            };
            
            var result = await _client.GetPreSignedURLAsync(presignedRequest);
    
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fail to upload file in minio with path {path} in bucket {bucket}",
                fileMetadata.FullPath,
                fileMetadata.BucketName);

            return Error.Failure("file.upload", "Fail to upload file in minio");
        }
    }
    
    public async Task<Result<List<string>>> GetPresignedUrlsForUploadParallel(
        IEnumerable<FileMetadata> fileMetadata,
        CancellationToken cancellationToken = default)
    {
        var fileMetadatas = fileMetadata.ToList();
        
        try
        {
            var results = new ConcurrentBag<string>();
            var errors = new ConcurrentBag<ErrorList>();

            await Parallel.ForEachAsync(fileMetadatas, new ParallelOptions
                {
                    MaxDegreeOfParallelism = MAX_DEGREE_OF_PARALLELISM,
                    CancellationToken = cancellationToken
                },
                async (metadata, token) =>
                {
                    var presignedRequest = new GetPreSignedUrlRequest
                    {
                        BucketName = metadata.BucketName,
                        Key = Uri.EscapeDataString(metadata.Key),
                        Verb = HttpVerb.PUT,
                        Expires = DateTime.UtcNow.AddDays(EXPIRATION_URL),
                        ContentType = metadata.ContentType,
                        Protocol = Protocol.HTTP
                    };

                    var result = await _client.GetPreSignedURLAsync(presignedRequest);

                    if (result is null)
                        errors.Add(Error.NotFound("object.not.found", 
                            "File does`t exist in minio"));
                    else
                        results.Add(result);
                });
            
            if (errors.Any())
                return Error.Failure("file.upload", $"Failed to upload {errors.Count} files");
    
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fail to upload files in minio");

            return Error.Failure("files.upload", "Fail to upload files in minio");
        }
    }
    
    private async Task IsBucketExist(IEnumerable<string> bucketNames,CancellationToken cancellationToken)
    {
        HashSet<string> buckets = [..bucketNames];
        
        var response = await _client.ListBucketsAsync(cancellationToken);

        foreach (var request in from bucketName in buckets
                 let isExist = response.Buckets
                     .Exists(b => b.BucketName.Equals(bucketName, StringComparison.OrdinalIgnoreCase)) 
                 where !isExist select new PutBucketRequest
                 {
                     BucketName = bucketName
                 })
        {
            await _client.PutBucketAsync(request, cancellationToken);
        }
    }
}
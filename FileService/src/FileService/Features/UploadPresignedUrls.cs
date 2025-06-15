using System.ComponentModel.Design;
using FileService.Api.Endpoints;
using FileService.Application.Providers;
using FileService.Application.Repositories;
using FileService.Contract;
using FileService.Contract.Requests;
using FileService.Contract.Responses;
using FileService.Data.Models;
using FileService.Jobs;
using Hangfire;

namespace FileService.Features;

public static class UploadPresignedUrls
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("files/presigned-urls-for-uploading", Handler);
        }
    }

    private static async Task<IResult> Handler(
        UploadPresignedUrlsRequest request,
        IFilesDataRepository repository,
        IFileProvider fileProvider,
        CancellationToken cancellationToken = default)
    {
        var records = (from metadata in request.Files
            let id = Guid.NewGuid()
            let extension = Path.GetExtension(metadata.FileName)
            let fileKey = $"{id}{extension}"
            select new FileMetadata
            {
                Id = id,
                BucketName = metadata.BucketName,
                ContentType = metadata.ContentType,
                FileName = metadata.FileName,
                Key = fileKey,
                Extension = extension,
                CreatedAt = DateTime.UtcNow
            }).ToList();

        var result = await fileProvider.GetPresignedUrlsForUploadParallel(records, cancellationToken);
        if (result.IsFailure)
            return Results.Problem(result.Errors.FirstOrDefault()!.ErrorMessage);

        await repository.AddRangeAsync(records, cancellationToken);

        BackgroundJob.Schedule<ConfirmConsistencyJob>(
            j => j.Execute(records, cancellationToken),
            TimeSpan.FromMinutes(3));

        var responses = result.Value.Select((url, i) =>
            new GetUploadPresignedUrlResponse(
            records[i].Id,
            records[i].Extension,
            url)).ToList();

        return Results.Ok(responses);
    }
}
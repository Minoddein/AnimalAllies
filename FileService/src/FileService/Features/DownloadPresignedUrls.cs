using FileService.Api.Endpoints;
using FileService.Application.Providers;
using FileService.Application.Repositories;
using FileService.Contract;
using FileService.Contract.Requests;
using FileService.Contract.Responses;
using FileService.Data.Models;

namespace FileService.Features;

public static class DownloadPresignedUrls
{
    public record FileKey(string FileId, string Extension);
    public record ManyDownloadPresignedUrlRequest(string BucketName, IEnumerable<FileKey> FileKeys);
    
    public sealed class Endpoint: IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("files/many-presigned-for-downloading", Handler);
        }
    }

    private static async Task<IResult> Handler( 
        ManyDownloadPresignedUrlRequest request,
        IFilesDataRepository filesDataRepository,
        IFileProvider provider,
        CancellationToken cancellationToken = default)
    {
        List<FileMetadata> filesMetadata = [];

        foreach (var key in request.FileKeys)
        {
            var fileMetadata = new FileMetadata
            {
                BucketName = request.BucketName,
                Key = $"{key.FileId}.{key.Extension}",
            };
            
            filesMetadata.Add(fileMetadata);
        }
        
        var result = await provider.DownloadFiles(filesMetadata, cancellationToken); 
        if(result.IsFailure)
            return Results.BadRequest(result.Errors);
        
        return Results.Ok(result.Value);
    }
}
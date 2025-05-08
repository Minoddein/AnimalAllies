namespace AnimalAllies.Core.DTOs.FileService;

public record UploadFileDto(
    string BucketName,
    string FileName,
    string ContentType);
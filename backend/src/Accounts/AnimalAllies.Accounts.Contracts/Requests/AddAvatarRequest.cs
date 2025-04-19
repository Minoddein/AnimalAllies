

namespace AnimalAllies.Accounts.Contracts.Requests;

public record UploadFileDto(
    string BucketName,
    string FileName, 
    string ContentType);

public record AddAvatarRequest(Guid UserId, UploadFileDto UploadFileDto);
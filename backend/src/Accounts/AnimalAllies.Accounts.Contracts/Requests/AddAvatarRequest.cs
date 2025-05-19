

namespace AnimalAllies.Accounts.Contracts.Requests;

public record UploadFileRequest(
    string BucketName,
    string FileName, 
    string ContentType);

public record AddAvatarRequest(UploadFileRequest UploadFileDto);
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.FileService;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddAvatar;

public record AddAvatarCommand(Guid UserId, UploadFileDto UploadFileDto) : ICommand;
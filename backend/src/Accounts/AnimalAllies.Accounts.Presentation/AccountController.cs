using AnimalAllies.Accounts.Application.AccountManagement.Commands.AddAvatar;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.AddSocialNetworks;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.ConfirmEmail;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.DeleteRefreshSession;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.Login;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.Refresh;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.Register;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.SetNotificationSettings;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUserById;
using AnimalAllies.Accounts.Contracts.Requests;
using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Models;
using AnimalAllies.SharedKernel.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FullNameDto = AnimalAllies.Core.DTOs.ValueObjects.FullNameDto;
using SocialNetworkDto = AnimalAllies.Core.DTOs.ValueObjects.SocialNetworkDto;
using UploadFileDto = AnimalAllies.Core.DTOs.FileService.UploadFileDto;

namespace AnimalAllies.Accounts.Presentation;

public class AccountController : ApplicationController
{
    [HttpPost("registration")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] RegisterUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        RegisterUserCommand command = new(
            request.Email,
            request.UserName,
            new FullNameDto(request.FirstName, request.SecondName, request.Patronymic),
            request.Password);

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.IsSuccess);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] Guid userId,
        [FromQuery] string code,
        [FromServices] ConfirmEmailHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || string.IsNullOrEmpty(code))
        {
            return BadRequest("Invalid parameters");
        }

        ConfirmEmailCommand request = new(userId, code);

        Result result = await handler.Handle(request, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [HttpPost("authentication")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] LoginUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        LoginUserCommand command = new(request.Email, request.Password);

        Result<LoginResponse> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        HttpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken.ToString());

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteRefreshTokenHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
        {
            return Unauthorized();
        }

        DeleteRefreshTokenCommand command = new(Guid.Parse(refreshToken));

        HttpContext.Response.Cookies.Delete("refreshToken");

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [HttpPost("refreshing")]
    public async Task<IActionResult> Refresh(
        [FromServices] RefreshTokensHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
        {
            return Unauthorized();
        }

        Result<LoginResponse> result = await handler.Handle(
            new RefreshTokensCommand(Guid.Parse(refreshToken)),
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        HttpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken.ToString());

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("notifications-settings")]
    public async Task<IActionResult> SetNewNotificationSettings(
        [FromBody] SetNotificationSettingsRequest request,
        [FromServices] SetNotificationSettingsHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        SetNotificationSettingsCommand command = new(
            userScopedData.UserId,
            request.EmailNotifications,
            request.TelegramNotifications,
            request.WebNotifications);

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.IsSuccess);
    }

    [Authorize]
    [HttpPost("social-networks-to-user")]
    public async Task<IActionResult> AddSocialNetworksToUser(
        [FromBody] AddSocialNetworksRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] AddSocialNetworkHandler handler,
        CancellationToken cancellationToken = default)
    {
        AddSocialNetworkCommand command = new(userScopedData.UserId, request.SocialNetworkRequests
            .Select(s => new SocialNetworkDto { Title = s.Title, Url = s.Url }));

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.IsSuccess);
    }

    [Authorize]
    [HttpPost("avatar")]
    public async Task<IActionResult> AddAvatarToUser(
        [FromBody] AddAvatarRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] AddAvatarHandler handler,
        CancellationToken cancellationToken = default)
    {
        AddAvatarCommand command = new(userScopedData.UserId,
            new UploadFileDto(
                request.UploadFileDto.BucketName,
                request.UploadFileDto.FileName,
                request.UploadFileDto.ContentType));

        Result<AddAvatarResponse> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult> Get(
        [FromRoute] Guid userId,
        [FromServices] GetUserByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetUserByIdQuery query = new(userId);

        Result<UserDto?> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }
}
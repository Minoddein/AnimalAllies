using AnimalAllies.Accounts.Application.AccountManagement.Commands.AddAvatar;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.BanUser;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.ConfirmEmail;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.DeleteRefreshSession;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.Login;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.Refresh;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.Register;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.SetNotificationSettings;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateCertificates;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateInfo;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateRequisites;
using AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateSocialNetworks;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUserById;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUsersCount;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUsersWithPagination;
using AnimalAllies.Accounts.Application.DTOs;
using AnimalAllies.Accounts.Contracts.Requests;
using AnimalAllies.Accounts.Presentation.Requests;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.Framework.Models;
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
        var command = new RegisterUserCommand(
            request.Email,
            request.UserName,
            new FullNameDto(request.FirstName, request.SecondName, request.Patronymic),
            request.Password);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

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
            return BadRequest("Invalid parameters");

        var request = new ConfirmEmailCommand(userId, code);

        var result = await handler.Handle(request, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        //TODO: в опции
        return Redirect("http://localhost:3000");
    }

    [HttpPost("authentication")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] LoginUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginUserCommand(request.Email, request.Password);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        HttpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken.ToString());

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteRefreshTokenHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }

        var command = new DeleteRefreshTokenCommand(Guid.Parse(refreshToken));

        HttpContext.Response.Cookies.Delete("refreshToken");

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result);
    }

    [HttpPost("refreshing")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromServices] RefreshTokensHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await handler.Handle(
            new RefreshTokensCommand(Guid.Parse(refreshToken)),
            cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new SetNotificationSettingsCommand(
            userScopedData.UserId,
            request.EmailNotifications,
            request.TelegramNotifications,
            request.WebNotifications);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result.IsSuccess);
    }

    [Authorize]
    [HttpPost("social-networks-to-user")]
    public async Task<IActionResult> AddSocialNetworksToUser(
        [FromBody] UpdateSocialNetworksRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateSocialNetworkHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSocialNetworkCommand(
            userScopedData.UserId,
            request.SocialNetworkRequests
                .Select(s => new SocialNetworkDto
                {
                    Title = s.Title,
                    Url = s.Url,
                }));

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result.IsSuccess);
    }

    [Authorize]
    [HttpPost("certificates-to-user")]
    public async Task<IActionResult> AddCertificatesToUser(
        [FromBody] UpdateCertificatesRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateCertificatesHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCertificatesCommand(userScopedData.UserId,
            request.Certificates
                .Select(c =>
                    new CertificateDto(
                        c.Title,
                        c.IssuingOrganization,
                        c.IssueDate,
                        c.ExpirationDate,
                        c.Description)));

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result.IsSuccess);
    }

    [Authorize]
    [HttpPost("requisite-to-user")]
    public async Task<IActionResult> UpdateRequisiteToUser(
        [FromBody] UpdateRequisitesRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateRequisitesHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateRequisitesCommand(userScopedData.UserId,
            request.Requisites
                .Select(c =>
                    new RequisiteDto
                    {
                        Title = c.Title,
                        Description = c.Description
                    }));

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result.IsSuccess);
    }

    [Authorize]
    [HttpPost("updating-info")]
    public async Task<IActionResult> UpdateInfoOfUser(
        [FromBody] UpdateInfoRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateInfoHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateInfoCommand(
            userScopedData.UserId,
            request.FirstName,
            request.SecondName,
            request.Patronymic,
            request.Phone,
            request.Experience);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new AddAvatarCommand(userScopedData.UserId,
            new UploadFileDto(
                request.UploadFileDto.BucketName,
                request.UploadFileDto.FileName,
                request.UploadFileDto.ContentType));

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult> Get(
        [FromRoute] Guid userId,
        [FromServices] GetUserByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("all-users-by-page")]
    public async Task<ActionResult> GetUsersWithPagination(
        [FromQuery] GetUsersWithPaginationRequest request,
        [FromServices] GetUsersWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersWithPaginationQuery(request.Page, request.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("ban-user")]
    public async Task<ActionResult> BanUser(
        [FromBody] BanUserRequest request,
        [FromServices] BanUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new BanUserCommand(request.UserId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetUsersCount(
        [FromServices] GetUsersCountHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersCountQuery();

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
}
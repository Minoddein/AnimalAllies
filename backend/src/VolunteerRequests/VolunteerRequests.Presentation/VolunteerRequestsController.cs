using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Models;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.Framework.Models;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using Microsoft.AspNetCore.Mvc;
using VolunteerRequests.Application.Features.Commands.ApproveVolunteerRequest;
using VolunteerRequests.Application.Features.Commands.CreateVolunteerRequest;
using VolunteerRequests.Application.Features.Commands.RejectVolunteerRequest;
using VolunteerRequests.Application.Features.Commands.ResendVolunteerRequest;
using VolunteerRequests.Application.Features.Commands.SendRequestForRevision;
using VolunteerRequests.Application.Features.Commands.TakeRequestForSubmit;
using VolunteerRequests.Application.Features.Commands.UpdateVolunteerRequest;
using VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByAdminIdWithPagination;
using VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByUserIdWithPagination;
using VolunteerRequests.Application.Features.Queries.GetVolunteerRequestsInWaitingWithPagination;
using VolunteerRequests.Contracts.Requests;

namespace VolunteerRequests.Presentation;

public class VolunteerRequestsController : ApplicationController
{
    [Permission("volunteerRequests.create")]
    [HttpPost("creation-volunteer-request")]
    public async Task<IActionResult> CreateVolunteerRequest(
        [FromBody] CreateVolunteerRequestRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] CreateVolunteerRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        CreateVolunteerRequestCommand command = new(
            userScopedData.UserId,
            new FullNameDto(request.FirstName, request.SecondName, request.Patronymic),
            request.Email,
            request.PhoneNumber,
            request.WorkExperience,
            request.VolunteerDescription,
            request.SocialNetworks.Select(s =>
                new SocialNetworkDto { Title = s.Title, Url = s.Url }));

        Result<VolunteerRequestId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.review")]
    [HttpPost("{volunteerRequestId:guid}/taking-request-for-submitting")]
    public async Task<IActionResult> TakeRequestForSubmit(
        [FromRoute] Guid volunteerRequestId,
        [FromServices] UserScopedData userScopedData,
        [FromServices] TakeRequestForSubmitHandler handler,
        CancellationToken cancellationToken = default)
    {
        TakeRequestForSubmitCommand command = new(userScopedData.UserId, volunteerRequestId);

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.review")]
    [HttpPost("sending-for-revision")]
    public async Task<IActionResult> SendRequestForRevision(
        [FromBody] SendRequestForRevisionRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] SendRequestForRevisionHandler handler,
        CancellationToken cancellationToken = default)
    {
        SendRequestForRevisionCommand command = new(
            userScopedData.UserId, request.VolunteerRequestId, request.RejectComment);

        Result<VolunteerRequestId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.review")]
    [HttpPost("rejecting-request")]
    public async Task<IActionResult> RejectRequest(
        [FromBody] RejectVolunteerRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] RejectVolunteerRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        RejectVolunteerRequestCommand command = new(
            userScopedData.UserId, request.VolunteerRequestId, request.RejectComment);

        Result<string> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.review")]
    [HttpPost("{volunteerRequestId:guid}approving-request")]
    public async Task<IActionResult> ApproveRequest(
        [FromRoute] Guid volunteerRequestId,
        [FromServices] UserScopedData userScopedData,
        [FromServices] ApproveVolunteerRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        ApproveVolunteerRequestCommand command = new(userScopedData.UserId, volunteerRequestId);

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.read")]
    [HttpGet("volunteer-requests-in-waiting-with-pagination")]
    public async Task<ActionResult> GetVolunteerRequestsInWaiting(
        [FromQuery] GetVolunteerRequestsInWaitingWithPaginationRequest request,
        [FromServices] GetVolunteerRequestsInWaitingWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetVolunteerRequestsInWaitingWithPaginationQuery query = new(
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

        Result<PagedList<VolunteerRequestDto>> result =
            await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.read")]
    [HttpGet("filtered-volunteer-requests-by-admin-id-with-pagination")]
    public async Task<ActionResult> GetFilteredVolunteerRequestsByAdminId(
        [FromQuery] GetFilteredRequestsByAdminIdRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] GetFilteredVolunteerRequestsByAdminIdWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery query = new(
            userScopedData.UserId,
            request.RequestStatus,
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

        Result<PagedList<VolunteerRequestDto>> result =
            await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.readOwn")]
    [HttpGet("filtered-volunteer-requests-by-user-id-with-pagination")]
    public async Task<ActionResult> GetFilteredVolunteerRequestsByUserId(
        [FromQuery] GetFilteredRequestsByUserIdRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] GetFilteredVolunteerRequestsByUserIdWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetFilteredVolunteerRequestsByUserIdWithPaginationQuery query = new(
            userScopedData.UserId,
            request.RequestStatus,
            request.SortBy,
            request.SortDirection,
            request.Page,
            request.PageSize);

        Result<PagedList<VolunteerRequestDto>> result =
            await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.update")]
    [HttpPut("update-volunteer-request")]
    public async Task<ActionResult> UpdateVolunteerRequest(
        [FromBody] UpdateRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateVolunteerRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        UpdateVolunteerRequestCommand command = new(
            userScopedData.UserId,
            request.VolunteerRequestId,
            new FullNameDto(request.FirstName, request.SecondName, request.Patronymic),
            request.Email,
            request.PhoneNumber,
            request.WorkExperience,
            request.VolunteerDescription,
            request.SocialNetworks.Select(s =>
                new SocialNetworkDto { Title = s.Title, Url = s.Url }));

        Result<VolunteerRequestId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteerRequests.update")]
    [HttpPut("{volunteerRequestId:guid}/resending-volunteer-request")]
    public async Task<ActionResult> ResendVolunteerRequest(
        [FromRoute] Guid volunteerRequestId,
        [FromServices] UserScopedData userScopedData,
        [FromServices] ResendVolunteerRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        ResendVolunteerRequestCommand query = new(userScopedData.UserId, volunteerRequestId);

        Result<VolunteerRequestId> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }
}
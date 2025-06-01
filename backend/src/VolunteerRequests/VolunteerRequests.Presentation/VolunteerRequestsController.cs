using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.Framework.Models;
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
using VolunteerRequests.Application.Features.Queries.GetVolunteerRequestByRelationUser;
using VolunteerRequests.Application.Features.Queries.GetVolunteerRequestInWaitingCount;
using VolunteerRequests.Application.Features.Queries.GetVolunteerRequestsInWaitingWithPagination;
using VolunteerRequests.Contracts.Requests;

namespace VolunteerRequests.Presentation;

public class VolunteerRequestsController: ApplicationController
{
    [Permission("volunteerRequests.create")]
    [HttpPost("creation-volunteer-request")]
    public async Task<IActionResult> CreateVolunteerRequest(
        [FromBody] CreateVolunteerRequestRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] CreateVolunteerRequestHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateVolunteerRequestCommand(
            userScopedData.UserId,
            new FullNameDto(request.FirstName, request.SecondName,  request.Patronymic), 
            request.Email,
            request.PhoneNumber,
            request.WorkExperience,
            request.VolunteerDescription);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new TakeRequestForSubmitCommand(userScopedData.UserId, volunteerRequestId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
           return result.Errors.ToResponse();

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
        var command = new SendRequestForRevisionCommand(
            userScopedData.UserId, request.VolunteerRequestId, request.RejectComment);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new RejectVolunteerRequestCommand(
            userScopedData.UserId, request.VolunteerRequestId, request.RejectComment);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new ApproveVolunteerRequestCommand(userScopedData.UserId, volunteerRequestId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteerRequests.read")]
    [HttpGet("volunteer-requests-in-waiting-with-pagination")]
    public async Task<ActionResult> GetVolunteerRequestsInWaiting(
        [FromQuery] GetVolunteerRequestsInWaitingWithPaginationRequest request,
        [FromServices] GetVolunteerRequestsInWaitingWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVolunteerRequestsInWaitingWithPaginationQuery(
            request.SortBy,
            request.SortDirection,
            request.Page, 
            request.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteerRequests.readOwn")]
    [HttpGet("volunteer-requests-by-relation-user")]
    public async Task<ActionResult> GetVolunteerRequestByRelationUser(
        [FromServices] UserScopedData userScopedData,
        [FromServices] GetVolunteerRequestByRelationUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVolunteerRequestByRelationUserQuery(userScopedData.UserId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

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
        var query = new GetFilteredVolunteerRequestsByAdminIdWithPaginationQuery(
            userScopedData.UserId,
            request.RequestStatus,
            request.SortBy,
            request.SortDirection,
            request.Page, 
            request.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

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
        var query = new GetFilteredVolunteerRequestsByUserIdWithPaginationQuery(
            userScopedData.UserId,
            request.RequestStatus,
            request.SortBy,
            request.SortDirection,
            request.Page, 
            request.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteerRequests.read")]
    [HttpGet("request-count")]
    public async Task<ActionResult> GetVolunteerRequestsInWaitingCount(
        [FromServices] GetVolunteerRequestInWaitingCountHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVolunteerRequestInWaitingCountQuery();

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

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
        var command = new UpdateVolunteerRequestCommand(
            userScopedData.UserId,
            request.VolunteerRequestId,
            new FullNameDto(request.FirstName, request.SecondName,  request.Patronymic), 
            request.Email,
            request.PhoneNumber,
            request.WorkExperience,
            request.VolunteerDescription);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

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
        var query = new ResendVolunteerRequestCommand(userScopedData.UserId, volunteerRequestId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
}
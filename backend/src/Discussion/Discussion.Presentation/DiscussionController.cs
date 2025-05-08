using AnimalAllies.Core.DTOs;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.Framework.Models;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using Discussion.Application.Features.Commands.CloseDiscussion;
using Discussion.Application.Features.Commands.DeleteMessage;
using Discussion.Application.Features.Commands.PostMessage;
using Discussion.Application.Features.Commands.UpdateMessage;
using Discussion.Application.Features.Queries.GetDiscussionByRelationId;
using Discussion.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Presentation;

public class DiscussionController : ApplicationController
{
    [Permission("discussion.create")]
    [HttpPost("posting-message")]
    public async Task<IActionResult> PostMessage(
        [FromBody] PostMessageRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] PostMessageHandler handler,
        CancellationToken cancellationToken = default)
    {
        PostMessageCommand command = new(request.DiscussionId, userScopedData.UserId, request.Text);

        Result<MessageId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("discussion.delete")]
    [HttpPost("deletion-message")]
    public async Task<IActionResult> DeleteMessage(
        [FromBody] DeleteMessageRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] DeleteMessageHandler handler,
        CancellationToken cancellationToken = default)
    {
        DeleteMessageCommand command = new(request.DiscussionId, userScopedData.UserId, request.MessageId);

        Result result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("discussion.update")]
    [HttpPut("editing-message")]
    public async Task<IActionResult> UpdateMessage(
        [FromBody] UpdateMessageRequest request,
        [FromServices] UserScopedData userScopedData,
        [FromServices] UpdateMessageHandler handler,
        CancellationToken cancellationToken = default)
    {
        UpdateMessageCommand command = new(
            request.DiscussionId, userScopedData.UserId, request.MessageId, request.Text);

        Result<MessageId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("discussion.update")]
    [HttpPut("{discussionId:guid}/closing-discussion")]
    public async Task<IActionResult> CloseDiscussion(
        [FromRoute] Guid discussionId,
        [FromServices] UserScopedData userScopedData,
        [FromServices] CloseDiscussionHandler handler,
        CancellationToken cancellationToken = default)
    {
        CloseDiscussionCommand command = new(discussionId, userScopedData.UserId);

        Result<DiscussionId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("discussion.read")]
    [HttpGet("messages-by-relation-id")]
    public async Task<IActionResult> GetMessagesByRelationId(
        [FromQuery] GetMessagesByRelationIdRequest request,
        [FromServices] GetDiscussionByRelationIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetDiscussionByRelationIdQuery query = new(request.RelationId, request.PageSize);

        Result<List<MessageDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }
}
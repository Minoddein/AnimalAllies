using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using Discussion.Domain.DomainEvents;
using Discussion.Domain.Entities;
using Discussion.Domain.ValueObjects;

namespace Discussion.Domain.Aggregate;

public class Discussion : DomainEntity<DiscussionId>
{
    private readonly List<Message> _messages = [];

    private Discussion(DiscussionId id)
        : base(id)
    {
    }

    private Discussion(DiscussionId id, Users users, Guid relationId)
        : base(id)
    {
        Users = users;
        RelationId = relationId;
        DiscussionStatus = DiscussionStatus.Open;

        CreatedDiscussionDomainEvent @event = new(relationId);

        AddDomainEvent(@event);
    }

    public DiscussionStatus DiscussionStatus { get; private set; }

    public Users Users { get; }

    public Guid RelationId { get; }

    public IReadOnlyList<Message> Messages => _messages;

    public static Result<Discussion> Create(DiscussionId id, Users users, Guid relationId)
    {
        if (relationId == Guid.Empty)
        {
            return Errors.General.Null("relation id");
        }

        return new Discussion(id, users, relationId);
    }

    public Result SendComment(Message message)
    {
        if (Users.FirstMember != message.UserId && Users.SecondMember != message.UserId)
        {
            return Error.Failure(
                "access.denied",
                "Send comment can user that take part in discussion");
        }

        _messages.Add(message);

        PostedMessageDomainEvent @event = new(RelationId);

        AddDomainEvent(@event);

        return Result.Success();
    }

    public Result DeleteComment(Guid userId, MessageId messageId)
    {
        Result<Message> message = GetMessageById(messageId);
        if (message.IsFailure)
        {
            return message.Errors;
        }

        if (message.Value.UserId != userId)
        {
            return Error.Failure(
                "access.denied",
                "Delete comment can user that sent this message");
        }

        _messages.Remove(message.Value);

        DeletedMessageDomainEvent @event = new(RelationId);

        AddDomainEvent(@event);

        return Result.Success();
    }

    public Result EditComment(Guid userId, MessageId messageId, Text text)
    {
        Result<Message> message = GetMessageById(messageId);
        if (message.IsFailure)
        {
            return message.Errors;
        }

        if (message.Value.UserId != userId)
        {
            return Error.Failure(
                "access.denied",
                "Edit comment can user that sent this message");
        }

        message.Value.Edit(text);

        UpdatedMessageDomainEvent @event = new(RelationId);

        AddDomainEvent(@event);

        return Result.Success();
    }

    public Result CloseDiscussion(Guid userId)
    {
        if (DiscussionStatus == DiscussionStatus.Closed)
        {
            return Error.Failure("discussion.status", "Discussion is already closed");
        }

        if (Users.FirstMember != userId && Users.SecondMember != userId)
        {
            return Error.Failure(
                "access.denied",
                "close discussion can user that take part in discussion");
        }

        DiscussionStatus = DiscussionStatus.Closed;

        ClosedDiscussionDomainEvent @event = new(RelationId);

        AddDomainEvent(@event);

        return Result.Success();
    }

    private Result<Message> GetMessageById(MessageId messageId)
    {
        Message? message = Messages.FirstOrDefault(m => m.Id == messageId);

        if (message is null)
        {
            return Errors.General.NotFound(messageId.Id);
        }

        return message;
    }
}
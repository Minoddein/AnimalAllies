﻿using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using Discussion.Domain.DomainEvents;
using Discussion.Domain.Entities;
using Discussion.Domain.ValueObjects;

namespace Discussion.Domain.Aggregate;

public class Discussion: DomainEntity<DiscussionId>
{
    private List<Message> _messages = [];
    private Discussion(DiscussionId id) : base(id){}

    private Discussion(DiscussionId id, Users users, Guid relationId) : base(id)
    {
        Users = users;
        RelationId = relationId;
        DiscussionStatus = DiscussionStatus.Open;
        
        var @event = new CreatedDiscussionDomainEvent(relationId);
        
        AddDomainEvent(@event);
    }
    
    public DiscussionStatus DiscussionStatus { get; private set; }
    public Message? LastMessage { get; private set; }
    public Users Users { get; private set; }
    public Guid RelationId { get; private set; }
    public IReadOnlyList<Message> Messages => _messages;
    
    public static Result<Discussion> Create(DiscussionId id, Users users, Guid relationId)
    {
        if (relationId == Guid.Empty)
            return Errors.General.Null("relation id");
        
        return new Discussion(id, users, relationId);
    }

    public Result SetLastMessageToDiscussion()
    {
        var lastMessage = _messages.LastOrDefault();
        if (lastMessage == null)
        {
            return Result.Success();
        }

        LastMessage = lastMessage;
        
        return Result.Success();
    }
    
    public Result MarkMessageAsRead(Guid userId)
    {
        if (!Users.IsOneOf(userId))
        {
            return Error.Failure("not.correct.user",
                "User with this id is not member of this discussion");
        }

        var unreadMessages = _messages.Where(m =>
            m.UserId != userId && m.IsRead == false)
            .ToList();
        
        unreadMessages.ForEach(m => m.MarkAsRead());
        
        return Result.Success();
    }

    public Result<int> GetUnreadMessagesCount(Guid userId)
    {
        if (!Users.IsOneOf(userId))
        {
            return Error.Failure("not.correct.user",
                "User with this id is not member of this discussion");
        }
        
        var unreadMessagesCount = _messages.Count(m => m.UserId != userId && m.IsRead == false);

        return unreadMessagesCount;
    }

    public Result SendComment(Message message)
    {
        if (Users.FirstMember != message.UserId && Users.SecondMember != message.UserId)
            return Error.Failure("access.denied",
                "Send comment can user that take part in discussion");
        
        _messages.Add(message);
        
        var @event = new PostedMessageDomainEvent(RelationId);
        
        AddDomainEvent(@event);

        SetLastMessageToDiscussion();
        
        return Result.Success();
    }

    public Result DeleteComment(Guid userId, MessageId messageId)
    {
        var message = GetMessageById(messageId);
        if (message.IsFailure)
            return message.Errors;

        if (message.Value.UserId != userId)
            return Error.Failure("access.denied",
                "Delete comment can user that sent this message");

        _messages.Remove(message.Value);
        
        var @event = new DeletedMessageDomainEvent(RelationId);
        
        AddDomainEvent(@event);

        SetLastMessageToDiscussion();

        return Result.Success();
    }
    
    public Result EditComment(Guid userId, MessageId messageId, Text text)
    {
        var message = GetMessageById(messageId);
        if (message.IsFailure)
            return message.Errors;

        if (message.Value.UserId != userId)
            return Error.Failure("access.denied",
                "Edit comment can user that sent this message");

        message.Value.Edit(text);
        
        var @event = new UpdatedMessageDomainEvent(RelationId);
        
        AddDomainEvent(@event);

        SetLastMessageToDiscussion();

        return Result.Success();
    }

    public Result CloseDiscussion(Guid userId)
    {
        if (DiscussionStatus == DiscussionStatus.Closed)
            return Error.Failure("discussion.status", "Discussion is already closed");

        if (Users.FirstMember != userId && Users.SecondMember != userId)
            return Error.Failure("access.denied",
                "close discussion can user that take part in discussion");
        
        DiscussionStatus = DiscussionStatus.Closed;
        
        var @event = new ClosedDiscussionDomainEvent(RelationId);
        
        AddDomainEvent(@event);
        
        return Result.Success();
    }
    

    private Result<Message> GetMessageById(MessageId messageId)
    {
        var message = Messages.FirstOrDefault(m => m.Id == messageId);

        if (message is null)
            return Errors.General.NotFound(messageId.Id);
        return message;
    }
}
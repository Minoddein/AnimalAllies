using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Discussion.Domain.ValueObjects;

namespace Discussion.Domain.Entities;

public class Message: DomainEntity<MessageId>
{
    private Message(MessageId id) : base(id){}

    private Message(
        MessageId id, 
        Text text,
        CreatedAt createdAt,
        IsEdited isEdited, 
        IsRead isRead, 
        Guid userId) : base(id)
    {
        Text = text;
        CreatedAt = createdAt;
        IsEdited = isEdited;
        UserId = userId;
        IsRead = isRead;
    }

    public static Result<Message> Create(
        MessageId id,
        Text text,
        CreatedAt createdAt, 
        IsEdited isEdited,
        IsRead isRead,
        Guid userId)
    {
        if (userId == Guid.Empty)
            return Errors.General.Null("user id");

        return new Message(id, text, createdAt, isEdited, isRead, userId);
    }

    public void Edit(Text text)
    {
        Text = text;
        IsEdited = new IsEdited(true);
    }

    public void MarkAsRead()
    {
        IsRead = new IsRead(true);
    }
    
    public Text Text { get; private set; }
    public CreatedAt CreatedAt { get; private set; }
    public IsEdited IsEdited { get; private set; }
    public IsRead IsRead { get; private set; }
    public Guid UserId { get; private set; }
    
}
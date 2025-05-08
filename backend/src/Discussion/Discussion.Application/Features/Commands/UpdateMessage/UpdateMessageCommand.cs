using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Commands.UpdateMessage;

public record UpdateMessageCommand(Guid DiscussionId, Guid UserId, Guid MessageId, string Text) : ICommand;
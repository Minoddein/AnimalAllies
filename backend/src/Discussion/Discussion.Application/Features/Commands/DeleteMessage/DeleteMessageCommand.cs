using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Commands.DeleteMessage;

public record DeleteMessageCommand(Guid DiscussionId, Guid UserId, Guid MessageId) : ICommand;
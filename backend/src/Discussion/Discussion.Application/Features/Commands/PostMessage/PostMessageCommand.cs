using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Commands.PostMessage;

public record PostMessageCommand(Guid DiscussionId, Guid UserId, string Text) : ICommand;
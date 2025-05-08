using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Commands.CloseDiscussion;

public record CloseDiscussionCommand(Guid DiscussionId, Guid UserId) : ICommand;
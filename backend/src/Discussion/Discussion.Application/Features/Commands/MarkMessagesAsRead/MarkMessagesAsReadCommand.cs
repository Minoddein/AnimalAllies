using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Commands.MarkMessagesAsRead;

public record MarkMessagesAsReadCommand(Guid UserId, Guid DiscussionId): ICommand;
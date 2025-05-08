using AnimalAllies.Core.Abstractions;

namespace Discussion.Application.Features.Commands.CreateDiscussion;

public record CreateDiscussionCommand(Guid FirstMember, Guid SecondMember, Guid RelationId) : ICommand;
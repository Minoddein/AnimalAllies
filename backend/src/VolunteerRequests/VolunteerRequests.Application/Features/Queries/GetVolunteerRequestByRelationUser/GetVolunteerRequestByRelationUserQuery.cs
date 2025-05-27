using AnimalAllies.Core.Abstractions;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestByRelationUser;

public record GetVolunteerRequestByRelationUserQuery(Guid UserId) : IQuery;

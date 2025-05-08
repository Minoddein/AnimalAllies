namespace AnimalAllies.Volunteer.Contracts.Requests;

public record GetPetsByBreedIdRequest(Guid BreedId, int Page, int PageSize);
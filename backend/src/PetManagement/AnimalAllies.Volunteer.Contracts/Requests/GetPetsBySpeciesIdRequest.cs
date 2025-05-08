namespace AnimalAllies.Volunteer.Contracts.Requests;

public record GetPetsBySpeciesIdRequest(Guid SpeciesId, int Page, int PageSize);
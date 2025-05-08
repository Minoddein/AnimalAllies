namespace AnimalAllies.Volunteer.Contracts.Responses;

public record DeletePetPhotosResponse(IEnumerable<string> FileUrls);
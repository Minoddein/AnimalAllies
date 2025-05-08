namespace AnimalAllies.Volunteer.Contracts.Responses;

public record AddPetPhotosResponse(IEnumerable<string> FileUrls);
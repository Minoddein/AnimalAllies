namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record DeletePetPhotosRequest(IEnumerable<string> FilePaths);
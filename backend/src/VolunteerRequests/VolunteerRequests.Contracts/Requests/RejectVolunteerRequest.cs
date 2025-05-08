namespace VolunteerRequests.Contracts.Requests;

public record RejectVolunteerRequest(Guid VolunteerRequestId, string RejectComment);
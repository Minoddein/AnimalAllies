namespace VolunteerRequests.Contracts.Requests;

public record SendRequestForRevisionRequest(Guid VolunteerRequestId, string RejectComment);
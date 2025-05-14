namespace AnimalAllies.Accounts.Contracts.Responses;

public record VolunteerAccountResponse(
    Guid Id,
    IEnumerable<CertificateResponse> Certificates,
    IEnumerable<RequisiteResponse> Requisites,
    int Experience);
    
public record CertificateResponse(
    string Title,
    string IssuingOrganization,
    DateTime IssueDate,
    DateTime ExpirationDate,
    string Description);

public record RequisiteResponse(
    string Title,
    string Description);
    
namespace AnimalAllies.Accounts.Contracts.Requests;

public record UpdateCertificatesRequest(IEnumerable<CertificateRequest> Certificates);

public record CertificateRequest(
    string Title,
    string IssuingOrganization,
    DateTime IssueDate,
    DateTime ExpirationDate,
    string Description);

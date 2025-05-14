namespace AnimalAllies.Accounts.Contracts.Requests;

public record AddCertificatesRequest(IEnumerable<CertificateRequest> Certificates);

public record CertificateRequest(
    string Title,
    string IssuingOrganization,
    DateTime IssueDate,
    DateTime ExpirationDate,
    string Description);

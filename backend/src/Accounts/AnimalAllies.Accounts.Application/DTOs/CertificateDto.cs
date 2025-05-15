namespace AnimalAllies.Accounts.Application.DTOs;

public record CertificateDto(
    string Title,
    string IssuingOrganization,
    DateTime IssueDate,
    DateTime ExpirationDate,
    string Description);
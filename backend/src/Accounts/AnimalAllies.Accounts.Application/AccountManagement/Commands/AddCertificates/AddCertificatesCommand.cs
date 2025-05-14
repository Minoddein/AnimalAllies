using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddCertificates;

public record AddCertificatesCommand(Guid UserId, IEnumerable<CertificateDto> Certificates) : ICommand;


public record CertificateDto(
    string Title,
    string IssuingOrganization,
    DateTime IssueDate,
    DateTime ExpirationDate,
    string Description);
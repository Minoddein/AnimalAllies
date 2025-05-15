using AnimalAllies.Accounts.Application.DTOs;
using AnimalAllies.Core.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateCertificates;

public record UpdateCertificatesCommand(Guid UserId, IEnumerable<CertificateDto> Certificates) : ICommand;
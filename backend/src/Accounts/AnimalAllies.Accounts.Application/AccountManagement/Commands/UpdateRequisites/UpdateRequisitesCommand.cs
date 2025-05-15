using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateRequisites;

public record UpdateRequisitesCommand(Guid UserId, IEnumerable<RequisiteDto> Requisites): ICommand;

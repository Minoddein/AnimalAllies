using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Accounts.Application.Managers;

public interface IAccountManager
{
    Task CreateAdminAccount(AdminProfile adminProfile);

    Task<Result> CreateParticipantAccount(
        ParticipantAccount participantAccount, CancellationToken cancellationToken = default);

    Task<Result> CreateVolunteerAccount(
        VolunteerAccount volunteerAccount, CancellationToken cancellationToken = default);
}
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;

namespace AnimalAllies.Volunteer.Application.Repository;

public interface IVolunteerRepository
{
    Task<Result<VolunteerId>> Create(
        Domain.VolunteerManagement.Aggregate.Volunteer entity,
        CancellationToken cancellationToken = default);

    Task<Result<VolunteerId>> Delete(
        Domain.VolunteerManagement.Aggregate.Volunteer entity,
        CancellationToken cancellationToken = default);

    Task<Result<VolunteerId>> Save(
        Domain.VolunteerManagement.Aggregate.Volunteer entity,
        CancellationToken cancellationToken = default);

    Task<Result<Domain.VolunteerManagement.Aggregate.Volunteer>> GetById(
        VolunteerId id,
        CancellationToken cancellationToken = default);

    Task<Result<Domain.VolunteerManagement.Aggregate.Volunteer>> GetByPhoneNumber(
        PhoneNumber phone,
        CancellationToken cancellationToken = default);

    Task<Result<Domain.VolunteerManagement.Aggregate.Volunteer>> GetByEmail(
        Email email,
        CancellationToken cancellationToken = default);

    Task<Result<List<Domain.VolunteerManagement.Aggregate.Volunteer>>> Get(
        CancellationToken cancellationToken = default);
}
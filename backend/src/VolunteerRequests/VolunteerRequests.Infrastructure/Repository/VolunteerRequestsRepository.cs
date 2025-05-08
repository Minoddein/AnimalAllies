using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;
using VolunteerRequests.Infrastructure.DbContexts;

namespace VolunteerRequests.Infrastructure.Repository;

public class VolunteerRequestsRepository(WriteDbContext context) : IVolunteerRequestsRepository
{
    private readonly WriteDbContext _context = context;

    public async Task<Result<VolunteerRequestId>> Create(
        VolunteerRequest entity, CancellationToken cancellationToken = default)
    {
        await _context.VolunteerRequests.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }

    public async Task<Result<VolunteerRequest>> GetById(
        VolunteerRequestId id,
        CancellationToken cancellationToken = default)
    {
        VolunteerRequest? volunteerRequest = await _context.VolunteerRequests
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken).ConfigureAwait(false);

        if (volunteerRequest == null)
        {
            return Errors.General.NotFound();
        }

        return volunteerRequest;
    }

    public async Task<Result<IReadOnlyList<VolunteerRequest>>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.VolunteerRequests
            .Where(v => v.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public Result<VolunteerRequestId> Delete(VolunteerRequest entity)
    {
        _context.VolunteerRequests.Remove(entity);

        return entity.Id;
    }
}
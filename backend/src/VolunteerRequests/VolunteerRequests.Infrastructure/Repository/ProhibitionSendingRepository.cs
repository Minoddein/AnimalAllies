using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using VolunteerRequests.Application.Repository;
using VolunteerRequests.Domain.Aggregates;
using VolunteerRequests.Infrastructure.DbContexts;

namespace VolunteerRequests.Infrastructure.Repository;

public class ProhibitionSendingRepository(WriteDbContext context) : IProhibitionSendingRepository
{
    public async Task<Result<ProhibitionSendingId>> Create(
        ProhibitionSending entity, CancellationToken cancellationToken = default)
    {
        await context.ProhibitionsSending.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }

    public async Task<Result<ProhibitionSending?>> GetById(
        ProhibitionSendingId id, CancellationToken cancellationToken = default) =>
        await context.ProhibitionsSending
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task<Result<ProhibitionSending>> GetByUserId(
        Guid userId, CancellationToken cancellationToken = default)
    {
        ProhibitionSending? result = await context.ProhibitionsSending
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Errors.General.NotFound(userId);
        }

        return result;
    }

    public Result Delete(ProhibitionSending entity)
    {
        context.ProhibitionsSending.Remove(entity);

        return Result.Success();
    }
}
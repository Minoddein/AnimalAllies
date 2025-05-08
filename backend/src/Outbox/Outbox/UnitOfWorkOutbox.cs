using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Outbox.Abstractions;
using Outbox.Outbox;

namespace Outbox;

public class UnitOfWorkOutbox(OutboxContext dbContext) : IUnitOfWorkOutbox
{
    private readonly OutboxContext _dbContext = dbContext;

    public async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        return transaction.GetDbTransaction();
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
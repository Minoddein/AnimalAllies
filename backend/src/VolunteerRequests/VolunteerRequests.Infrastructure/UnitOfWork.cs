using System.Data;
using AnimalAllies.Core.Database;
using Microsoft.EntityFrameworkCore.Storage;
using VolunteerRequests.Infrastructure.DbContexts;

namespace VolunteerRequests.Infrastructure;

public class UnitOfWork(WriteDbContext dbContext) : IUnitOfWork
{
    private readonly WriteDbContext _dbContext = dbContext;

    public async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        return transaction.GetDbTransaction();
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
using System.Data;
using AnimalAllies.Core.Database;
using AnimalAllies.Species.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace AnimalAllies.Species.Infrastructure;

public class UnitOfWork(SpeciesWriteDbContext dbContext) : IUnitOfWork
{
    private readonly SpeciesWriteDbContext _dbContext = dbContext;

    public async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        return transaction.GetDbTransaction();
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
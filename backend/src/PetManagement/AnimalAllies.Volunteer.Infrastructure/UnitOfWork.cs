using System.Data;
using AnimalAllies.Core.Database;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace AnimalAllies.Volunteer.Infrastructure;

public class UnitOfWork(VolunteerWriteDbContext dbContext) : IUnitOfWork
{
    private readonly VolunteerWriteDbContext _dbContext = dbContext;

    public async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        return transaction.GetDbTransaction();
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
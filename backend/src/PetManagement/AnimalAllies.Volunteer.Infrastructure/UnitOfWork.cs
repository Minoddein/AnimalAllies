using System.Data;
using AnimalAllies.Core.Database;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace AnimalAllies.Volunteer.Infrastructure;

public class UnitOfWork: IUnitOfWork
{
    private readonly VolunteerWriteDbContext _dbContext;

    public UnitOfWork(VolunteerWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        return transaction.GetDbTransaction();
    }

    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
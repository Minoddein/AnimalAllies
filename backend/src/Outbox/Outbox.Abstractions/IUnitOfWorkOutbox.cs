using System.Data;

namespace Outbox.Abstractions;

public interface IUnitOfWorkOutbox
{
    Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default);

    Task SaveChanges(CancellationToken cancellationToken = default);
}
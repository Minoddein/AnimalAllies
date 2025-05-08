using AnimalAllies.Core.Options;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AnimalAllies.Volunteer.Infrastructure.Services;

public class DeleteExpiredPetsService(
    VolunteerWriteDbContext dbContext,
    IOptions<EntityDeletion> entityDeletion)
{
    private readonly VolunteerWriteDbContext _dbContext = dbContext;
    private readonly EntityDeletion _entityDeletion = entityDeletion.Value;

    public async Task Process(CancellationToken cancellationToken = default)
    {
        IEnumerable<Domain.VolunteerManagement.Aggregate.Volunteer> volunteers =
            await GetVolunteersWithPets(cancellationToken).ConfigureAwait(false);
        foreach (Domain.VolunteerManagement.Aggregate.Volunteer volunteer in volunteers)
        {
            volunteer.DeleteExpiredPets(_entityDeletion.ExpiredTime);
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IEnumerable<Domain.VolunteerManagement.Aggregate.Volunteer>> GetVolunteersWithPets(
        CancellationToken cancellationToken) =>
        await _dbContext.Volunteers
            .Include(v => v.Pets).ToListAsync(cancellationToken).ConfigureAwait(false);
}
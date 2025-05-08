using AnimalAllies.Core.Options;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AnimalAllies.Volunteer.Infrastructure.Services;

public class DeleteExpiredVolunteerService(
    VolunteerWriteDbContext dbContext,
    IOptions<EntityDeletion> entityDeletion)
{
    private readonly VolunteerWriteDbContext _dbContext = dbContext;
    private readonly EntityDeletion _entityDeletion = entityDeletion.Value;

    public async Task Process(CancellationToken cancellationToken = default)
    {
        List<Domain.VolunteerManagement.Aggregate.Volunteer> volunteers = await _dbContext.Volunteers
            .Where(v => v.IsDeleted).ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (Domain.VolunteerManagement.Aggregate.Volunteer volunteer in volunteers)
        {
            if (volunteer.DeletionDate!.Value.AddDays(_entityDeletion.ExpiredTime) <= DateTime.UtcNow)
            {
                _dbContext.Volunteers.Remove(volunteer);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
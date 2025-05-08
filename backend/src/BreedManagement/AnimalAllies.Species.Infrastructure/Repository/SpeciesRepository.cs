using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Species.Application.Repository;
using AnimalAllies.Species.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AnimalAllies.Species.Infrastructure.Repository;

public class SpeciesRepository(SpeciesWriteDbContext context) : ISpeciesRepository
{
    private readonly SpeciesWriteDbContext _context = context;

    public async Task<Result<SpeciesId>> Create(Domain.Species entity, CancellationToken cancellationToken = default)
    {
        await _context.Species.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }

    public Result<SpeciesId> Delete(Domain.Species entity, CancellationToken cancellationToken = default)
    {
        _context.Species.Remove(entity);

        return entity.Id;
    }

    public Result<SpeciesId> Save(Domain.Species entity, CancellationToken cancellationToken = default)
    {
        _context.Species.Attach(entity);

        return entity.Id;
    }

    public async Task<Result<Domain.Species>> GetById(SpeciesId id, CancellationToken cancellationToken = default)
    {
        Domain.Species? species = await _context.Species
            .Include(s => s.Breeds)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken).ConfigureAwait(false);

        if (species == null)
        {
            return Result<Domain.Species>.Failure(Errors.General.NotFound());
        }

        return Result<Domain.Species>.Success(species);
    }

    public async Task<Result<List<Domain.Species>>> Get(CancellationToken cancellationToken = default) =>
        await _context.Species
            .Include(s => s.Breeds)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
}
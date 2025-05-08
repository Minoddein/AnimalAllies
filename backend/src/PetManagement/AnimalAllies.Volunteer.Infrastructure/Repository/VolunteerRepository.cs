using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AnimalAllies.Volunteer.Infrastructure.Repository;

public class VolunteerRepository(VolunteerWriteDbContext context) : IVolunteerRepository
{
    private readonly VolunteerWriteDbContext _context = context;

    public async Task<Result<VolunteerId>> Create(
        Domain.VolunteerManagement.Aggregate.Volunteer entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Volunteers.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }

    public async Task<Result<VolunteerId>> Delete(
        Domain.VolunteerManagement.Aggregate.Volunteer entity,
        CancellationToken cancellationToken = default)
    {
        _context.Volunteers.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }

    public async Task<Result<VolunteerId>> Save(
        Domain.VolunteerManagement.Aggregate.Volunteer entity,
        CancellationToken cancellationToken = default)
    {
        _context.Volunteers.Attach(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }

    public async Task<Result<Domain.VolunteerManagement.Aggregate.Volunteer>> GetById(
        VolunteerId id,
        CancellationToken cancellationToken = default)
    {
        Domain.VolunteerManagement.Aggregate.Volunteer? volunteer = await _context.Volunteers
            .Include(x => x.Pets)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

        if (volunteer == null)
        {
            return Result<Domain.VolunteerManagement.Aggregate.Volunteer>.Failure(Errors.General.NotFound(id.Id));
        }

        return Result<Domain.VolunteerManagement.Aggregate.Volunteer>.Success(volunteer);
    }

    public async Task<Result<Domain.VolunteerManagement.Aggregate.Volunteer>> GetByPhoneNumber(
        PhoneNumber phone, CancellationToken cancellationToken = default)
    {
        Domain.VolunteerManagement.Aggregate.Volunteer? volunteer = await _context.Volunteers
            .Include(x => x.Pets)
            .FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken).ConfigureAwait(false);

        if (volunteer == null)
        {
            return Result<Domain.VolunteerManagement.Aggregate.Volunteer>.Failure(Errors.General.NotFound());
        }

        return Result<Domain.VolunteerManagement.Aggregate.Volunteer>.Success(volunteer);
    }

    public async Task<Result<Domain.VolunteerManagement.Aggregate.Volunteer>> GetByEmail(
        Email email,
        CancellationToken cancellationToken = default)
    {
        Domain.VolunteerManagement.Aggregate.Volunteer? volunteer = await _context.Volunteers
            .Include(x => x.Pets)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);

        if (volunteer == null)
        {
            return Result<Domain.VolunteerManagement.Aggregate.Volunteer>.Failure(Errors.General.NotFound());
        }

        return Result<Domain.VolunteerManagement.Aggregate.Volunteer>.Success(volunteer);
    }

    public async Task<Result<List<Domain.VolunteerManagement.Aggregate.Volunteer>>> Get(
        CancellationToken cancellationToken = default)
    {
        List<Domain.VolunteerManagement.Aggregate.Volunteer> volunteers = await _context.Volunteers
            .Include(v => v.Pets)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return Result<List<Domain.VolunteerManagement.Aggregate.Volunteer>>.Success(volunteers);
    }
}
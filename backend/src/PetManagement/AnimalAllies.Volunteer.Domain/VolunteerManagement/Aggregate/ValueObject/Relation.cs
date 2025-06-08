using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;

public class Relation: SharedKernel.Shared.Objects.ValueObject
{
    public Guid RelationId { get; }

    private Relation(Guid relationId)
    {
        RelationId = relationId;
    }

    public static Result<Relation> Create(Guid relationId)
    {
        if (Guid.Empty == relationId)
        {
            return Errors.General.ValueIsRequired(nameof(relationId));
        }

        return new Relation(relationId);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RelationId;
    }
}
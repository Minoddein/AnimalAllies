using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.Ids;

public class BaseId<TId> : ValueObject
    where TId : notnull
{
    protected BaseId(Guid id) => Id = id;

    public Guid Id { get; }

    public static TId NewGuid() => Create(Guid.NewGuid());

    public static TId Empty() => Create(Guid.Empty);

    public static TId Create(Guid id) => (TId)Activator.CreateInstance(typeof(TId), id)!;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
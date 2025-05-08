using AnimalAllies.SharedKernel.Shared.Ids;

namespace AnimalAllies.SharedKernel.Shared.Objects;

/// <summary>
///     Абстрактный класс для реализации сущности с доменными событиями.
/// </summary>
/// <typeparam name="TId">Обобщенный класс Id.</typeparam>
public abstract class DomainEntity<TId>(TId id) : Entity<TId>(id)
    where TId : BaseId<TId>
{
    private readonly Queue<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary>
    ///     Добавить событие в очередь.
    /// </summary>
    /// <param name="event">доменное событие.</param>
    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Enqueue(@event);

    /// <summary>
    ///     Удаление события из очереди.
    /// </summary>
    /// <param name="event">доменное событие.</param>
    public void RemoveDomainEvent(IDomainEvent @event)
        => _domainEvents.TryDequeue(out _);

    /// <summary>
    ///     Очистка всех событий в очереди.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
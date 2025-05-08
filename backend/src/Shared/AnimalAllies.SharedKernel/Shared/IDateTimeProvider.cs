namespace AnimalAllies.SharedKernel.Shared;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
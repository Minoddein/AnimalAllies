using AnimalAllies.SharedKernel.Shared;

namespace AnimalAllies.Core.Common;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
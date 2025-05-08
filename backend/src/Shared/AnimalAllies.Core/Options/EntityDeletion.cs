namespace AnimalAllies.Core.Options;

public class EntityDeletion
{
    public static string ENTITY_DELETION = nameof(ENTITY_DELETION);

    public int ExpiredTime { get; init; }
}
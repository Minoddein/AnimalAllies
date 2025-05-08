namespace AnimalAllies.Core.Options;

public class JwtOptions
{
    public static string JWT = nameof(JWT);

    public required string Audience { get; init; }

    public required string Issuer { get; init; }

    public required string Key { get; init; }

    public required string ExpiredMinutesTime { get; init; }
}
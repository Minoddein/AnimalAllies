namespace AnimalAllies.Accounts.Domain;

public class RefreshSession
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public User User { get; init; } = default!;

    public Guid Jti { get; init; }

    public Guid RefreshToken { get; init; }

    public DateTime ExpiresIn { get; init; }

    public DateTime CreatedAt { get; init; }
}
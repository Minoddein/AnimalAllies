namespace AnimalAllies.Core.DTOs.Accounts;

public class ProhibitionSendingDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime BannedAt { get; set; }
}
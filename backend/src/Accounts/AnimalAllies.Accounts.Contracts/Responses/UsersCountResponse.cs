namespace AnimalAllies.Accounts.Contracts.Responses;

public record UsersCountResponse(int TotalUsers, int ActiveUsers, int BlockedUsers, int VolunteerUsers)
{
    public UsersCountResponse() : this(0, 0, 0, 0) { }
}

using AnimalAllies.SharedKernel.Shared.ValueObjects;

namespace AnimalAllies.Accounts.Domain;

public class AdminProfile
{
    public const string ADMIN = "Admin";

    private AdminProfile() { }

    public AdminProfile(FullName fullName, User user)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        User = user;
    }

    public Guid Id { get; set; }

    public FullName FullName { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }
}
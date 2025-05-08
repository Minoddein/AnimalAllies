using AnimalAllies.SharedKernel.Shared.ValueObjects;

namespace AnimalAllies.Accounts.Domain;

public class VolunteerAccount
{
    public static readonly string Volunteer = nameof(Volunteer);
    private readonly List<Certificate> _certificates = [];
    private readonly List<Requisite> _requisites = [];

    private VolunteerAccount() { }

    public VolunteerAccount(FullName fullName, int experience, User user)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        Experience = experience;
        User = user;
    }

    public Guid Id { get; set; }

    public FullName FullName { get; set; }

    public int Experience { get; set; }

    public IReadOnlyList<Certificate> Certificates => _certificates;

    public IReadOnlyList<Requisite> Requisites => _requisites;

    public Guid UserId { get; set; }

    public User User { get; set; }

    public void AddCertificates(IEnumerable<Certificate> certificates) => _certificates.AddRange(certificates);

    public void AddRequisites(IEnumerable<Requisite> requisites) => _requisites.AddRange(requisites);
}
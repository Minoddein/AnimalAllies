
using AnimalAllies.SharedKernel.Shared.ValueObjects;

namespace AnimalAllies.Accounts.Domain;

public class VolunteerAccount
{
    public static readonly string Volunteer = nameof(Volunteer);
    private VolunteerAccount(){}

    public VolunteerAccount(FullName fullName,int experience, User user)
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
    private List<Certificate> _certificates = [];
    public IReadOnlyList<Requisite> Requisites => _requisites;
    private List<Requisite> _requisites = [];
    public Guid UserId { get; set; }
    public User User { get; set; }
    public PhoneNumber Phone { get; set; }

    public void UpdateCertificates(IEnumerable<Certificate> certificates)
    {
        _certificates = certificates.ToList();
    }

    public void UpdateRequisites(IEnumerable<Requisite> requisites)
    {
        _requisites = requisites.ToList();
    }
}
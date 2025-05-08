using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.ValueObjects;

public class VolunteerInfo : ValueObject
{
    private VolunteerInfo() { }

    public VolunteerInfo(
        FullName fullName,
        Email email,
        PhoneNumber phoneNumber,
        WorkExperience workExperience,
        VolunteerDescription volunteerDescription,
        IEnumerable<SocialNetwork> socialNetworks)
    {
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        WorkExperience = workExperience;
        VolunteerDescription = volunteerDescription;
        SocialNetworks = [.. socialNetworks];
    }

    public FullName FullName { get; }

    public Email Email { get; }

    public PhoneNumber PhoneNumber { get; }

    public WorkExperience WorkExperience { get; }

    public VolunteerDescription VolunteerDescription { get; }

    public IReadOnlyList<SocialNetwork> SocialNetworks { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return Email;
        yield return PhoneNumber;
        yield return WorkExperience;
        yield return VolunteerDescription;
        yield return SocialNetworks;
    }
}
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.ValueObjects;

public class VolunteerInfo : ValueObject
{
    public FullName FullName { get; }
    public Email Email { get; }
    public PhoneNumber PhoneNumber { get; }
    public WorkExperience WorkExperience { get; }
    public VolunteerDescription VolunteerDescription { get; }
    
    private VolunteerInfo(){}

    public VolunteerInfo(
        FullName fullName, 
        Email email, 
        PhoneNumber phoneNumber,
        WorkExperience workExperience,
        VolunteerDescription volunteerDescription)
    {
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        WorkExperience = workExperience;
        VolunteerDescription = volunteerDescription;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
        yield return Email;
        yield return PhoneNumber;
        yield return WorkExperience;
        yield return VolunteerDescription;
    }
}
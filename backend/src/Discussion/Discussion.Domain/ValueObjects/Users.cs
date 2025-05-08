using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace Discussion.Domain.ValueObjects;

public class Users : ValueObject
{
    private Users() { }

    private Users(Guid firstMember, Guid secondMember)
    {
        FirstMember = firstMember;
        SecondMember = secondMember;
    }

    public Guid FirstMember { get; }

    public Guid SecondMember { get; }

    public static Result<Users> Create(Guid firstMember, Guid secondMember)
    {
        if (firstMember == Guid.Empty || secondMember == Guid.Empty)
        {
            return Errors.General.Null("one of users ids");
        }

        return new Users(firstMember, secondMember);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstMember;
        yield return SecondMember;
    }
}
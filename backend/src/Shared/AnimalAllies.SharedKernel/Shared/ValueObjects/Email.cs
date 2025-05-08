using System.Text.RegularExpressions;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.ValueObjects;

public partial class Email : ValueObject
{
    public static readonly Regex ValidationRegex = MyRegex();

    private Email() { }

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string email)
    {
        if (!ValidationRegex.IsMatch(email))
        {
            return Errors.Errors.General.ValueIsInvalid(email);
        }

        return new Email(email);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[\w-\.]{1,40}@([\w-]+\.)+[\w-]{2,4}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}
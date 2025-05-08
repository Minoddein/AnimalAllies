using System.Text.RegularExpressions;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    public static readonly Regex ValidationRegex = MyRegex();

    private PhoneNumber() { }

    private PhoneNumber(string number) => Number = number;

    public string Number { get; }

    public static Result<PhoneNumber> Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number) || !ValidationRegex.IsMatch(number))
        {
            return Errors.Errors.General.ValueIsRequired(number);
        }

        PhoneNumber phoneNumber = new(number);

        return phoneNumber;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }

    [GeneratedRegex(@"(^\+\d{1,3}\d{10}$|^$)", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}
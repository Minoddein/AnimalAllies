using System.Text.RegularExpressions;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.ValueObjects;

public partial class FullName : ValueObject
{
    private static readonly Regex ValidationRegex = MyRegex();

    private FullName() { }

    private FullName(string firstName, string secondName, string? patronymic)
    {
        FirstName = firstName;
        SecondName = secondName;
        Patronymic = patronymic;
    }

    public string FirstName { get; }

    public string SecondName { get; }

    public string? Patronymic { get; }

    public void Deconstruct(out string firstName, out string secondName, out string? patronymic)
    {
        firstName = FirstName;
        secondName = SecondName;
        patronymic = Patronymic;
    }

    public static Result<FullName> Create(string firstName, string secondName, string? patronymic)
    {
        if (string.IsNullOrWhiteSpace(firstName) || !ValidationRegex.IsMatch(firstName))
        {
            return Errors.Errors.General.ValueIsInvalid(firstName);
        }

        if (string.IsNullOrWhiteSpace(secondName) || !ValidationRegex.IsMatch(secondName))
        {
            return Errors.Errors.General.ValueIsInvalid(secondName);
        }

        return new FullName(firstName, secondName, patronymic);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return SecondName;
        yield return Patronymic;
    }

    public override string ToString() => $"{SecondName} {FirstName} {Patronymic}";

    [GeneratedRegex(@"^[\p{L}\p{M}\p{N}]{1,50}\z", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}
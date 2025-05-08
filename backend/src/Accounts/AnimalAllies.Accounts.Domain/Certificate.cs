using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Accounts.Domain;

public class Certificate : ValueObject
{
    private Certificate() { }

    private Certificate(
        string title,
        string issuingOrganization,
        DateTime issueDate,
        DateTime expirationDate,
        string description)
    {
        Title = title;
        IssuingOrganization = issuingOrganization;
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        Description = description;
    }

    public string Title { get; set; } = string.Empty;

    public string IssuingOrganization { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public static Result<Certificate> Create(
        string title,
        string issuingOrganization,
        DateTime issueDate,
        DateTime expirationDate,
        string description)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid(title);
        }

        if (string.IsNullOrWhiteSpace(issuingOrganization) || issuingOrganization.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid(issuingOrganization);
        }

        if (issueDate.Date > DateTime.Now.Date.AddDays(1) || issueDate.Date > expirationDate.Date)
        {
            return Errors.General.ValueIsInvalid(nameof(issueDate));
        }

        if (issueDate.Date > expirationDate.Date)
        {
            return Errors.General.ValueIsInvalid(nameof(expirationDate));
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid(description);
        }

        return new Certificate(title, issuingOrganization, issueDate, expirationDate, description);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title;
        yield return IssuingOrganization;
        yield return IssueDate;
        yield return ExpirationDate;
        yield return Description;
    }
}
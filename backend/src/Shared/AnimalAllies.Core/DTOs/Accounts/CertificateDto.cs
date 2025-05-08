namespace AnimalAllies.Core.DTOs.Accounts;

public class CertificateDto
{
    public string Title { get; set; } = string.Empty;

    public string IssuingOrganization { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public string Description { get; set; } = string.Empty;
}
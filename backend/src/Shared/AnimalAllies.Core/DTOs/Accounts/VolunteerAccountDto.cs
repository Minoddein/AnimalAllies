using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs.Accounts;

public class VolunteerAccountDto
{
    public Guid VolunteerId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string SecondName { get; set; } = string.Empty;

    public string Patronymic { get; set; } = string.Empty;

    public RequisiteDto[] Requisites { get; set; } = [];

    public CertificateDto[] Certificates { get; set; } = [];

    public int Experience { get; set; }
}
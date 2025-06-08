namespace AnimalAllies.Accounts.Infrastructure.Options;

public class EmailConfirmOptions
{
    public const string EmailConfirm = "EmailConfirm";
    public string Url { get; set; } = string.Empty;
}
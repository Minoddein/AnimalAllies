namespace AnimalAllies.Accounts.Contracts.Requests;

public class SocialNetworkDto
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}


public record AddSocialNetworksRequest(IEnumerable<SocialNetworkDto> SocialNetworkDtos);
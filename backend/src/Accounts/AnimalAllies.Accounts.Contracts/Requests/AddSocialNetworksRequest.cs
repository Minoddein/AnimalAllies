namespace AnimalAllies.Accounts.Contracts.Requests;

public class SocialNetworkRequest
{
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}

public record AddSocialNetworksRequest(IEnumerable<SocialNetworkRequest> SocialNetworkRequests);
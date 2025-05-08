namespace AnimalAllies.SharedKernel.CachingConstants;

public static class TagsConstants
{
    public const string ACCOUNTS = "accounts";
    public const string USERS = "users";
    public const string PERMISSIONS = "permissions";
    public const string BREEDS = "breeds";
    public const string SPECIES = "species";
    public const string PETS = "pets";
    public const string VOLUNTEERS = "volunteers";
    public const string DISCUSSIONS = "discussions";
    public const string MESSAGES = "messages";
    public const string VOLUNTEER_REQUESTS = "volunteers-requests";

    public static class VolunteerRequests
    {
        public const string IN_WAITING = "in-waiting";
        public const string BY_ADMIN = "by-admin";
        public const string BY_USER = "by-user";
    }
}

public static class CacheChannels
{
    public const string CACHE_CHANNEL = "cache_invalidator_channel";
}
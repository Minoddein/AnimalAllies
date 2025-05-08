namespace AnimalAllies.Core.Options;

public class RefreshSessionOptions
{
    public static string REFRESH_SESSION = "RefreshSession";

    public int ExpiredDaysTime { get; set; }
}
using System.Text.RegularExpressions;

namespace AnimalAllies.SharedKernel.Constraints;

public static partial class Constraints
{
    public enum Context
    {
        PetManagement,
        BreedManagement,
        Accounts,
        Discussion,
        VolunteerRequests,
        Outbox
    }

    public static readonly int MAX_VALUE_LENGTH = 100;
    public static readonly int MAX_DESCRIPTION_LENGTH = 1500;
    public static readonly int MAX_PET_COLOR_LENGTH = 50;
    public static readonly int MAX_PET_INFORMATION_LENGTH = 2000;
    public static readonly double MIN_VALUE = 0;
    public static readonly int MIN_EXP_VALUE = 0;
    public static readonly int MAX_URL_LENGTH = 2048;
    public static readonly int MAX_PATH_LENGHT = 260;
    public static readonly int MAX_EXP_VALUE = 90;
    public static readonly int MAX_PHONENUMBER_LENGTH = 14;
    public static readonly int MIDDLE_NAME_LENGTH = 50;
    public static readonly int MIN_LENGTH_PASSWORD = 8;

    public static string[] Extensions = [".jpg", ".png", ".jpeg", ".svg"];
    public static string[] HELP_STATUS_PET_FROM_VOLUNTEER = ["SearchingHome", "FoundHome"];

    public static readonly Regex ValidationRegex = MyRegex();

    public static readonly string BUCKET_NAME = "photos";

    [GeneratedRegex(@"^[\w-\.]{1,40}@([\w-]+\.)+[\w-]{2,4}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}
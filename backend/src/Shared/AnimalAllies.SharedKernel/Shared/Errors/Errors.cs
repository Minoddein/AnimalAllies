namespace AnimalAllies.SharedKernel.Shared.Errors;

public static class Errors
{
    public static class General
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            string label = name ?? "value";
            return Error.Validation("Invalid.input", $"{label} is invalid");
        }

        public static Error NotFound(Guid? id = null)
        {
            string forId = id == null ? string.Empty : $"for Id:{id}";
            return Error.NotFound("Record.not.found", $"record not found {forId}");
        }

        public static Error Null(string? name = null)
        {
            string label = name ?? "value";
            return Error.Null("Null.entity", $"{label} is null");
        }

        public static Error ValueIsRequired(string? name = null)
        {
            string label = name == null ? string.Empty : " " + name + " ";
            return Error.Validation("Invalid.length", $"invalid{label}length");
        }

        public static Error AlreadyExist() => Error.Validation("Record.already.exist", "Records already exist");
    }

    public static class Tokens
    {
        public static Error ExpiredToken() => Error.Validation("token.is.expired", "Your token is expired");

        public static Error InvalidToken() => Error.Validation("token.is.invalid", "Your token is invalid");
    }

    public static class Volunteer
    {
        // TODO: Удалить и поменять на General
        public static Error AlreadyExist() => Error.Validation("Record.already.exist", "Volunteer already exist");

        public static Error PetPositionOutOfRange() =>
            Error.Validation("Position.out.of.range", "Pet position is out of range");
    }

    public static class User
    {
        public static Error InvalidCredentials() =>
            Error.Validation("credentials.is.invalid", "Your credentials is invalid");
    }

    public static class Species
    {
        public static Error DeleteConflict() => Error.Conflict(
            "Exist.dependent.records",
            "Cannot delete because there are records that depend on it");

        public static Error AlreadyExist() => Error.Validation("Record.already.exist", "Species already exist");

        public static Error BreedAlreadyExist() =>
            Error.Validation("Record.already.exist", "Breed of this species already exist");
    }
}
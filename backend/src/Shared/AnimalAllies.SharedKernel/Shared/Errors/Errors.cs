namespace AnimalAllies.SharedKernel.Shared.Errors;

public static class Errors
{
    public static class General
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            var label = name ?? "value";
            return Error.Validation("Invalid.input", $"{label} is invalid");
        }

        public static Error NotFound(Guid? id = null)
        {
            var forId = id == null ? "" : $"for Id:{id}";
            return Error.NotFound("Record.not.found", $"record not found {forId}");
        }

        public static Error Null(string? name = null)
        {
            var label = name ?? "value";
            return Error.Null("Null.entity", $"{label} is null");
        }

        public static Error ValueIsRequired(string? name = null)
        {
            var label = name == null ? "" : " " + name + " ";
            return Error.Validation("Invalid.length",$"invalid{label}length");
        }
        public static Error AlreadyExist()
        {
            return Error.Validation("Record.already.exist", $"Records already exist");
        }

        public static Error ValueTooLong(string? name = null)
        {
            return Error.Validation("value.too.long", $"Value {name} is too long");
        }
    }

    public static class Tokens
    {
        public static Error ExpiredToken()
        {
            return Error.Validation("token.is.expired", $"Your token is expired");
        }
        
        public static Error InvalidToken()
        {
            return Error.Validation("token.is.invalid", $"Your token is invalid");
        }
    }

    public static class Volunteer
    {
        //TODO: Удалить и поменять на General
        public static Error AlreadyExist()
        {
            return Error.Validation("Record.already.exist", $"Volunteer already exist");
        }

        public static Error PetPositionOutOfRange()
        {
            return Error.Validation("Position.out.of.range", "Pet position is out of range");
        }
    }

    public static class User
    {
        public static Error InvalidCredentials()
        {
            return Error.Validation("credentials.is.invalid", "Your credentials is invalid");
        }
    }
    
    public static class Species
    {
        public static Error DeleteConflict()
        {
            return Error.Conflict("Exist.dependent.records", "Cannot delete because there are records that depend on it");
        }
        
        public static Error AlreadyExist()
        {
            return Error.Validation("Record.already.exist", $"Species already exist");
        }
        
        public static Error BreedAlreadyExist()
        {
            return Error.Validation("Record.already.exist", $"Breed of this species already exist");
        }
    }
}
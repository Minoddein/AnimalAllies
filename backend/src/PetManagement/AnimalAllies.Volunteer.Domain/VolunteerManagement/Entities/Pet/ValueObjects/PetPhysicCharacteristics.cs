using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class PetPhysicCharacteristics : ValueObject
{
    public string Color { get; }
    public string HealthInformation { get; } 
    public double Weight { get; }
    public double Height { get; }
    
    private PetPhysicCharacteristics(){}

    private PetPhysicCharacteristics(
        string color,
        string healthInformation,
        double weight,
        double height)
    {
        Color = color;
        HealthInformation = healthInformation;
        Weight = weight;
        Height = height;
    }

    public static Result<PetPhysicCharacteristics> Create(
        string color,
        string healthInformation,
        double weight,
        double height)
    {
        
        if (string.IsNullOrWhiteSpace(color) || color.Length > Constraints.MAX_PET_COLOR_LENGTH)
        {
            return Errors.General.ValueIsRequired(color);
        }
        
        if (string.IsNullOrWhiteSpace(healthInformation) || healthInformation.Length > Constraints.MAX_PET_COLOR_LENGTH)
        {
            return Errors.General.ValueIsRequired(healthInformation);
        }
        
        if (weight < Constraints.MIN_VALUE)
        {
            return Errors.General.ValueIsInvalid(nameof(weight));
        }
        
        if (height < Constraints.MIN_VALUE)
        {
            return Errors.General.ValueIsRequired(nameof(height));
        }

        return (new PetPhysicCharacteristics(color, healthInformation, weight, height));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Color;
        yield return HealthInformation;
        yield return Weight;
        yield return Height;
    }
}
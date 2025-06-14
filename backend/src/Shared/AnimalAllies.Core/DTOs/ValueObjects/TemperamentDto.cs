namespace AnimalAllies.Core.DTOs.ValueObjects;

public record TemperamentDto(
    int? AggressionLevel,
    int? Friendliness,
    int? ActivityLevel,
    bool? GoodWithKids,
    bool? GoodWithPeople,
    bool? GoodWithOtherAnimals);
namespace AnimalAllies.Core.DTOs.ValueObjects;

public record HistoryDto(DateTime ArriveTime, string From, string? LastOwner);
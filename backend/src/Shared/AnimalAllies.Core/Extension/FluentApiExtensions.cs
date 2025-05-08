using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimalAllies.Core.Extension;

public static class FluentApiExtensions
{
    public static PropertyBuilder<IReadOnlyList<TValueObject>> ValueObjectJsonConverter<TValueObject, TDto>(
        this PropertyBuilder<IReadOnlyList<TValueObject>> builder,
        Func<TValueObject, TDto> toDtoSelector,
        Func<TDto, TValueObject> toValueObjectSelector) =>
        builder.HasConversion(
                valueObject => SerializeValueObjectCollection(valueObject, toDtoSelector),
                json => DeserializeDtoCollection(json, toValueObjectSelector),
                ValueComparer<TValueObject, TDto>())
            .HasColumnType("jsonb");

    private static ValueComparer ValueComparer<TValueObject, TDto>() =>
        new ValueComparer<IReadOnlyList<TValueObject>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
            c => c.ToList());

    private static string SerializeValueObjectCollection<TValueObject, TDto>(
        IReadOnlyCollection<TValueObject> valueObjects,
        Func<TValueObject, TDto> selector)
    {
        IEnumerable<TDto> dtos = valueObjects.Select(selector);

        return JsonSerializer.Serialize(dtos, JsonSerializerOptions.Default);
    }

    private static IReadOnlyList<TValueObject> DeserializeDtoCollection<TValueObject, TDto>(
        string json,
        Func<TDto, TValueObject> selector)
    {
        IEnumerable<TDto> dtos = JsonSerializer.Deserialize<IEnumerable<TDto>>(json, JsonSerializerOptions.Default) ??
                                 [];

        return [.. dtos.Select(selector)];
    }
}
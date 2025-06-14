using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AnimalAllies.Core.Extension;

public static class FluentApiExtensions
{
    public static PropertyBuilder<IReadOnlyList<TValueObject>> ValueObjectJsonConverter<TValueObject, TDto>(
        this PropertyBuilder<IReadOnlyList<TValueObject>> builder,
        Func<TValueObject, TDto> toDtoSelector,
        Func<TDto, TValueObject> toValueObjectSelector)
    {
        return builder.HasConversion(
                valueObject => SerializeValueObjectCollection(valueObject, toDtoSelector),
                json => DeserializeDtoCollection(json, toValueObjectSelector),
                ValueComparer<TValueObject, TDto>())
            .HasColumnType("jsonb");

    }

    private static ValueComparer ValueComparer<TValueObject, TDto>()
    {
        return new ValueComparer<IReadOnlyList<TValueObject>>(
            (c1,c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a,v!.GetHashCode())),
            c => c.ToList());
    }

    private static string SerializeValueObjectCollection<TValueObject,TDto>(
        IReadOnlyCollection<TValueObject> valueObjects,
        Func<TValueObject, TDto> selector)
    {
        var dtos = valueObjects.Select(selector);
        
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = false,
        };

        return JsonSerializer.Serialize(dtos, options);
    }
    
    private static IReadOnlyList<TValueObject> DeserializeDtoCollection<TValueObject,TDto>(
        string json,
        Func<TDto, TValueObject> selector)
    {
        if (string.IsNullOrEmpty(json))
            return new List<TValueObject>();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        var dtos = JsonSerializer.Deserialize<IEnumerable<TDto>>(json, options) ?? [];

        return dtos.Select(selector).ToList();
    }
    
}
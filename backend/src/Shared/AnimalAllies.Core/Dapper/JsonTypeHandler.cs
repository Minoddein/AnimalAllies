using System.Data;
using System.Text.Json;
using Dapper;

namespace AnimalAllies.Core.Dapper;

public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    public override void SetValue(IDbDataParameter parameter, T? value) =>
        parameter.Value = JsonSerializer.Serialize(value);

    public override T? Parse(object value) =>
        JsonSerializer.Deserialize<T>(value as string ?? string.Empty, JsonSerializerOptions.Default);
}
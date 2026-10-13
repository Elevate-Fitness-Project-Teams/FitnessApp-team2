using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitnessCalculationService.Common.Json;

public class EnumDescriptionConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<string, TEnum> _fromDescription = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<TEnum, string> _toDescription = new();

    public EnumDescriptionConverter()
    {
        var type = typeof(TEnum);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var value = (TEnum)field.GetValue(null)!;
            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
            var description = descriptionAttribute?.Description ?? field.Name;

            _fromDescription[description] = value;
            _fromDescription[field.Name] = value;
            _toDescription[value] = description;
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (!string.IsNullOrEmpty(stringValue) && _fromDescription.TryGetValue(stringValue, out var enumVal))
            {
                return enumVal;
            }
        }

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intVal))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), intVal);
        }

        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (_toDescription.TryGetValue(value, out var description))
        {
            writer.WriteStringValue(description);
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

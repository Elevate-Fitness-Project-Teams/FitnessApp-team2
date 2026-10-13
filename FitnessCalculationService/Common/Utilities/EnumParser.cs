using System.ComponentModel;
using System.Reflection;

namespace FitnessCalculationService.Common.Utilities;

public static class EnumParser
{
    public static bool TryParseDescription<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var clean = value.Trim();

        // 1. Try match by field name
        if (Enum.TryParse<TEnum>(clean, ignoreCase: true, out result))
            return true;

        // 2. Try match by [Description] attribute
        var type = typeof(TEnum);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null && string.Equals(descAttr.Description, clean, StringComparison.OrdinalIgnoreCase))
            {
                result = (TEnum)field.GetValue(null)!;
                return true;
            }
        }

        return false;
    }

    public static string GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var member = type.GetMember(value.ToString());
        if (member.Length > 0)
        {
            var desc = member[0].GetCustomAttribute<DescriptionAttribute>();
            if (desc != null)
                return desc.Description;
        }

        return value.ToString();
    }
}

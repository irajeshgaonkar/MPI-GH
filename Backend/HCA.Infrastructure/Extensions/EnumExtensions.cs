using HCA.Models.Enums;

namespace HCA.Infrastructure.Extensions;

public static class EnumExtensions
{
    public static string GetStringValue(this Enum val)
    {
        string output = string.Empty;
        var memberInfo = val.GetType().GetMember(val.ToString()).FirstOrDefault();
        if (null == memberInfo) return output;

        var attribute =  memberInfo.GetCustomAttributes(typeof(StringValueAttribute), false)
                               .FirstOrDefault();
        if (null == attribute) return output;
        var stringValueAttribute = attribute as StringValueAttribute;
        if (null == stringValueAttribute) return output;
        return stringValueAttribute.Value;
    }
}


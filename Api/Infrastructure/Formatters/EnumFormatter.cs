using System;
using System.ComponentModel;
using System.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.Formatters
{
    public static class EnumFormatter
    {
        public static string ToString<T>(T value) where T : Enum
        {
            var type = value.GetType();
            var targetName = value.ToString();
            
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var member = type.GetMember(name);
                if (!targetName.Equals(name))
                    continue;

                if (member[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute description)
                    return description.Description;
            }

            return targetName;
        }
    }
}

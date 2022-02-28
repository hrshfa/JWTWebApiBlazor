using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizeTest.Shared.Utils
{
    public static class PublicExtensions
    {
        public static string GetDescription(this Enum value)
        {

            FieldInfo? fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[]? attributes = fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        public static string GetPropertyValue(this object value, string propertyName)
        {
            PropertyInfo? pi = value.GetType().GetProperties()
                            .Where(a => a.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (pi == null)
            {
                return "";
            }
            object? propValue = pi?.GetGetMethod()?.Invoke(value, null);
            if (propValue == null)
            {
                return "";
            }
            if (pi?.PropertyType.IsEnum??false)
            {
                Enum e = (Enum)propValue;
                return e.GetDescription();
            }
            return propValue?.ToString()??"";
        }
    }
}

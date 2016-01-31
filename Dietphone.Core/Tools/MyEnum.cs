// Based on http://stackoverflow.com/questions/105372/c-how-to-enumerate-an-enum
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dietphone.Tools
{
    public static class MyEnum
    {
        public static IEnumerable<T> GetValues<T>()
        {
            var enumType = typeof(T);
            var fields = enumType.GetRuntimeFields();
            var enumFields = fields.Where(fieldInfo => fieldInfo.IsStatic && fieldInfo.IsPublic);
            var enumerations = enumFields.Select(fieldInfo => fieldInfo.GetValue(enumType));
            return enumerations.Cast<T>();
        }
    }
}
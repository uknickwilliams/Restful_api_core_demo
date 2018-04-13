using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Library.API.Helpers
{
    static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
        {
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var sourceType = typeof(TSource);
                var propertyInfos = fields.Split(',').Select(f => f.Trim())
                    .Select(p => sourceType.GetProperty(p, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance))
                    .Where(p => p != null)
                    .ToList();

                propertyInfoList.AddRange(propertyInfos);
            }

            var dataShapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(source);

                ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }
    }
}
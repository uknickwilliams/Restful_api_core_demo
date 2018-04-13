using System.Linq;
using System.Reflection;

namespace Library.API.Services
{
    public class TypeHelperService : ITypeHelperService
    {
        public bool TypeHasProperty<T>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }


            var sourceType = typeof(T);
            return fields.Split(',').Select(f => f.Trim())
                .Select(p => sourceType.GetProperty(p, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance))
                .All(p => p != null);
        }
    }
}
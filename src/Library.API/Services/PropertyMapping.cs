using System.Collections.Generic;

namespace Library.API.Services
{
    public interface IPropertyMapping
    {

    }
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        private readonly Dictionary<string, PropertyMappingValue> _mappingDictionary;

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _mappingDictionary = mappingDictionary;
        }

        public Dictionary<string, PropertyMappingValue> GetMappingDicitionary()
        {
            return _mappingDictionary;
        }
    }
}
using System.Collections.Generic;

namespace Library.API.Services
{
    public interface IPropertyMappingSerivce
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}
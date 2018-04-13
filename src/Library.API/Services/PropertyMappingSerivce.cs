using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingSerivce : IPropertyMappingSerivce
    {
        private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapiing =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new [] { "Id"})},
                { "Genre", new PropertyMappingValue(new [] { "Genre"})},
                { "Age", new PropertyMappingValue(new [] { "DateOfBirth"}, true)},
                { "Name", new PropertyMappingValue(new [] { "FirstName", "LastName"})},
            };

        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingSerivce()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapiing));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var match = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>().FirstOrDefault();
            if (match == null)
                throw new Exception("Cannot find exact propert mapping instance");

            return match.GetMappingDicitionary();
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
                return true;

            var fieldParts = fields.Split(',').Select(p => p.Trim());

            foreach (var fieldPart in fieldParts)
            {
                var spaceIndex = fieldPart.IndexOf(" ");
                var propertyName = spaceIndex == -1 ? fieldPart : fieldPart.Remove(spaceIndex);

                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
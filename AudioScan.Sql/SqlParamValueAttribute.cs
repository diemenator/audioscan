using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

namespace AudioScan.Sql
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlParamValueAttribute : Attribute
    {
        private static readonly Dictionary<Type, IDbValueConverter> Converters =
            new Dictionary<Type, IDbValueConverter>()
            {
                {typeof(StringToBytesConverter), new StringToBytesConverter()},
                {typeof(TimespanToMillisConverter), new TimespanToMillisConverter()}
            };

        public SqlParamValueAttribute(Type converterType = null, string name = null)
        {
            ConverterType = converterType;
            Name = name;

            if (ConverterType != null)
            {
                Converter = Converters[ConverterType];
            }
        }

        public string Name { get; private set; }

        public Type ConverterType { get; private set; }

        public IDbValueConverter Converter { get; private set; }

        public void AddWithValue(SqlParameterCollection targetCollection, PropertyInfo property, object instance)
        {
            var n = Name ?? ("@" + property.Name);

            var value = property.GetValue(instance);

            if (Converter != null)
            {
                value = Converter.Convert(value);
            }

            if (value == null)
            {
                value = DBNull.Value;
            }

            targetCollection.AddWithValue(n, value);
        }
    }
}
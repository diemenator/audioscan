using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AudioScan.Sql
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlColumnAttribute : Attribute
    {
        private static readonly Dictionary<Type, IDbValueConverter> Converters = new Dictionary<Type, IDbValueConverter>()
        {
            {typeof(BytesToToStringConverter), new BytesToToStringConverter() },
            {typeof(MillisToTimespanConverter), new MillisToTimespanConverter() }
        };

        public SqlColumnAttribute(Type converterType = null, string name = null)
        {
            ConverterType = converterType;
            Name = name;

            if (ConverterType != null)
            {
                Converter = Converters[ConverterType];
            }
        }
         
        public IDbValueConverter Converter { get; private set; }

        public string Name { get; private set; }

        public Type ConverterType { get; private set; }

        public Func<SqlDataReader, object, bool> Binder(PropertyInfo propertyInfo)
        {
            var n = Name ?? (propertyInfo.Name);

            var sourceType = propertyInfo.PropertyType;

            if (Converter != null)
            {
                sourceType = Converter.SourceType;
            }

            var hackTheTypesystem =
                typeof (SqlColumnAttribute).GetMethod("BindOrdinal")
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(sourceType);

            return (SqlDataReader source, object target) =>
            {
                var ordinal = source.GetOrdinal(n);

                if (ordinal < 0)
                {
                    return false;
                }
                try
                {
                    var value = hackTheTypesystem.Invoke(this, new object[] { ordinal, source });

                    if (Converter != null)
                    {
                        value = Converter.Convert(value);
                    }

                    if (value == null)
                    {
                        return false;
                    }

                    if (value is DateTime)
                    {
                        value = ((DateTime)value).ToUniversalTime();
                    }

                    propertyInfo.SetValue(target, value);
                }
                catch
                {
                    return false;
                }

                return true;
            };
        }

        public object BindOrdinal<T>(int ordinal, SqlDataReader source)
        {
            var isNull = source.IsDBNull(ordinal);

            if (isNull)
            {
                return null;
            }

            var result = source.GetFieldValue<T>(ordinal);

            return result;
        }
    }
}
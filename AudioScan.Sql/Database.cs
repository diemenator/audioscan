using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AudioScan.Sql
{
    public static class Database
    {
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(Settings.Default.Db.ConnectionString);
        }

        public static Action<T, SqlParameterCollection> Mapper<T>()
        {
            var mappers =
                typeof(T)
                .GetProperties()
                .SelectMany(property =>
                    property.GetCustomAttributes()
                    .OfType<SqlParamValueAttribute>()
                    .Select(attr =>
                    {
                        void it(object src, SqlParameterCollection sqlp)
                        {
                            attr.AddWithValue(sqlp, property, src);
                        }

                        return (Action<object, SqlParameterCollection>)it;
                    })
                );

            return (source, target) =>
            {
                foreach (var mapper in mappers)
                {
                    mapper(source, target);
                }
            };
        }

        public static Action<SqlDataReader, T> Reader<T>()
        {
            var mappers = 
                typeof(T)
                .GetProperties()
                .SelectMany(property =>
                    property
                    .GetCustomAttributes().OfType<SqlColumnAttribute>()
                    .Select(attr => attr.Binder(property))
                );

            return (SqlDataReader source, T target) =>
            {
                foreach (var mapper in mappers)
                {
                    mapper(source, target);
                }
            };
        }
    }
}
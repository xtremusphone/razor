using Factory;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Reflection;

namespace Factory.DB
{
    public static class SqlDataUtils
    {

        public static void SetParameters(this SQLiteCommand command, params SQLiteParameter[] parameters)
        {
            command.Parameters.AddRange(parameters);
        }

        //public static void SetParameters(this MySqlCommand command, params MySqlParameter[] parameters)
        //{
        //    command.Parameters.AddRange(parameters);
        //}

        public static T Get<T>(this DataRow row, string fieldName)
        {
            //Type listType = typeof(T);

            //object result = new object();
            //if (listType == typeof(int))
            //{
            //    result = Convert.ToInt32(row[fieldName]);
            //}
            //else if (listType == typeof(double))
            //{
            //    result = Convert.ToDouble(row[fieldName]);
            //}
            //else if (listType == typeof(decimal))
            //{
            //    result = Convert.ToDecimal(row[fieldName]);
            //}
            //else if (listType == typeof(DateTime))
            //{
            //    result = Convert.ToDateTime(row[fieldName]);
            //}
            return (T)Convert.ChangeType(row[fieldName], typeof(T));
        }

    }


    [Serializable]
    internal class SqlException : Exception
    {
        public SqlException()
        {
        }

        public SqlException(string? message) : base(message)
        {
        }

        public SqlException(string message, Exception innerException, string funcName, string query, DynamicSqlParameter? param = null) : base(message, innerException)
        {
            Log.Error("Function name :{funcName} - {error} ", funcName, innerException.Message);
            Log.Error("Query: {sql}", query);
            if (param != null) Log.Error("{param}", param.GetAsString());
        }

        protected SqlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public static class DataReaderExt
    {

        public static List<T> MapPropertyAttribute<T>(this DbDataReader dataReader) where T : new()
        {
            var mapResult = new List<T>();
            var propInfos = ReflectionFactory.GetMappableProperties<SqlPropertyAttribute>(typeof(T));
            var fieldNames = Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName).ToList();

            //var names = propInfos.Where(x=>x.MemberType==MemberTypes.Property).Select(x => x.GetCustomAttribute<SqlitePropertyAttribute>().PropertyName);
            //foreach (var name in names)
            //{
            //    Console.WriteLine(name);
            //}
            //var matchedProps = propInfos.Where(x => x.MemberType == MemberTypes.Property && fieldNames.Contains(x.GetCustomAttribute<SqlPropertyAttribute>().PropertyName));


            if (propInfos.Any())
                while (dataReader.Read())
                {
                    var obj = new T();

                    //foreach (var prop in propInfos)
                    //{
                    //    //Attribute.IsDefined(prop, typeof(SqlPropertyAttribute))
                    //    var attribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                    //    if (attribute!=null)
                    //    {
                    //        var colName = attribute.PropertyName;

                    //        if (dataReader[colName] == DBNull.Value) continue;

                    //        var val = dataReader[colName].GetType().Name == ReflectionFactory.GetTypeName(prop.PropertyType) ? dataReader[colName] : Convert.ChangeType(dataReader[colName], prop.PropertyType);
                    //        prop.SetValue(obj, val, null);
                    //    }
                    //}
                    foreach (var fieldName in fieldNames)
                    {
                        var prop = propInfos.FirstOrDefault(x => x.GetCustomAttribute<SqlPropertyAttribute>().PropertyName.Equals(fieldName));
                        if (prop != null)
                        {
                            if (dataReader[fieldName] == DBNull.Value) continue;

                            if (prop.PropertyType == typeof(DateTime?))
                            {
                                prop.SetValue(obj, dataReader[fieldName], null);
                            }
                            else
                            {
                                var val = dataReader[fieldName].GetType() == prop.PropertyType ? dataReader[fieldName] : Convert.ChangeType(dataReader[fieldName], prop.PropertyType);
                                prop.SetValue(obj, val, null);
                            }
                        }
                    }
                    mapResult.Add(obj);
                }

            return mapResult;
        }

        public static JArray MapToJsonArray(this DbDataReader dataReader)
        {
            var jarray = new JArray();
            var cols = Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName).ToList();
            while (dataReader.Read())
            {
                var obj = new JObject();

                foreach (string col in cols)
                {
                    obj[col] = dataReader[col].ToString();
                }
                jarray.Add(obj);
            }

            return jarray;
        }

        public static List<object> ToDataList(this DbDataReader dataReader)
        {
            var resultList = new List<object>();
            var cols = Enumerable.Range(0, dataReader.FieldCount).Select(dataReader.GetName).ToList();
            while (dataReader.Read())
            {
                dynamic expando = new ExpandoObject();

                foreach (string col in cols)
                {
                    //obj[col] = dataReader[col].ToString();
                    if (dataReader[col] == DBNull.Value)
                    {
                        AddProperty(expando, col, "");
                    }
                    else
                    {
                        var val = dataReader[col];
                        if (dataReader[col].GetType() == typeof(DateTime)) val = string.Format("{0:yyyy-MM-dd}", val);

                        AddProperty(expando, col, val);
                    }
                }
                resultList.Add(expando);
            }

            return resultList;
        }
        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

    }

}

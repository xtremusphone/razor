using Serilog;
using System.Reflection;

namespace Factory.DB
{
    internal class QueryFactory
    {
        internal static string CreateTable(Type type, string? table = "")
        {
            var tableNameT = ReflectionFactory.GetTableAttribute(type);
            if (string.IsNullOrEmpty(table) && string.IsNullOrEmpty(tableNameT)) throw new ArgumentNullException(nameof(table) + " cannot be null!");
            if (string.IsNullOrEmpty(table)) table = tableNameT;


            var queryColumns = new List<string>();
            var propInfos = ReflectionFactory.GetMappableProperties(type);
            foreach (var prop in propInfos)
            {
                var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                if (customAttribute == null) continue;

                var colName = customAttribute.PropertyName ?? prop.Name;
                var dataType = string.Empty;

                if (!string.IsNullOrEmpty(customAttribute.DataType))
                {
                    dataType = SqliteUtil.GetSqliteDataType(customAttribute.DataType);
                }
                else
                {
                    switch (ReflectionFactory.GetTypeName(prop.PropertyType).ToLower())
                    {
                        case "int32":
                            dataType = "INTEGER";
                            break;
                        case "string":
                            dataType = "VARCHAR(50)";
                            break;
                        case "int64":
                            dataType = "BIGINT";
                            break;
                        case "double":
                            dataType = "DOUBLE";
                            break;
                        case "decimal":
                            dataType = "DECIMAL";
                            break;
                        case "datetime":
                            dataType = "DATETIME";
                            break;
                        case "boolean":
                            dataType = "BOOL";
                            break;
                    }
                }

                if (customAttribute.DataSize > 0)
                {
                    dataType += $"({customAttribute.DataSize})";
                }

                var q = $"{colName} {dataType}";

                var pk = string.Empty;
                var ai = string.Empty;

                if (prop.GetCustomAttribute<SqlPrimaryKey>() != null)
                {
                    pk = SqliteUtil.PrimaryKey;
                }

                if (prop.GetCustomAttribute<SqlAutoIncrement>() != null)
                {
                    ai = SqliteUtil.Autoincrement;
                }

                q += $" {pk} {ai}";

                queryColumns.Add(q);
            }

            //var query = "insert into " + table + "(" + string.Join(",", queryColumns) + ") values (" + string.Join(",", queryParamBinders) + ")";

            var query = $"create table IF NOT EXISTS {table} ({string.Join(",", queryColumns)})";
            Log.Debug(query);
            return query;
        }

        internal static Tuple<string, DynamicSqlParameter> IsTableExists(string table)
        {
            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            var sqlParam = new DynamicSqlParameter();
            sqlParam.Add("tableName", table);

            return Tuple.Create(query, sqlParam);
        }

        internal static string TruncateTable(string table)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException(nameof(table) + " cannot be null!");
            return $"Delete from {table}";
        }

        internal static string DropTable(string table)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException(nameof(table) + " cannot be null!");
            return $"DROP table {table}";
        }

        internal static Tuple<string, DynamicSqlParameter> Delete<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null!");

            Type objType = obj.GetType();
            var table = ReflectionFactory.GetTableAttribute(objType);

            var sqlParam = new DynamicSqlParameter();
            var queryParamBinders = new List<string>();
            var whereQuery = new List<string>();
            var propInfos = ReflectionFactory.GetMappableProperties(objType);

            var primary = propInfos.Where(x => x.GetCustomAttribute<SqlPrimaryKey>() != null);

            foreach (var prop in primary)
            {
                var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                if (customAttribute == null) continue;

                var colName = customAttribute.PropertyName;

                var paramVal = prop.GetValue(obj);
                if (paramVal != null)
                {
                    var paramBinder = "@" + colName;

                    whereQuery.Add($"{colName}={paramBinder}");
                    sqlParam.Add(paramBinder, paramVal);
                }
            }

            var query = $"DELETE from {table} where {string.Join(" and ", whereQuery)}";
            Log.Debug(query);
            return Tuple.Create(query, sqlParam);
        }

        internal static Tuple<string, DynamicSqlParameter> Insert<T>(T obj, string? table = null)
        {
            var tableNameT = ReflectionFactory.GetTableAttribute(typeof(T));
            if (string.IsNullOrEmpty(table) && string.IsNullOrEmpty(tableNameT)) throw new ArgumentNullException(nameof(table) + " cannot be null!");
            if (string.IsNullOrEmpty(table)) table = tableNameT;

            var sqlParam = new DynamicSqlParameter();
            var queryColumns = new List<string>();
            var queryParamBinders = new List<string>();
            var propInfos = ReflectionFactory.GetMappableProperties(typeof(T));
            foreach (var prop in propInfos)
            {
                var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                if (customAttribute == null) continue;

                if (prop.GetCustomAttribute<SqlAutoIncrement>() != null) continue;

                var colName = customAttribute.PropertyName ?? prop.Name;
                var paramBinder = "@" + colName;
                var paramVal = prop.GetValue(obj);
                if (paramVal != null)
                {
                    queryColumns.Add(colName);
                    queryParamBinders.Add(paramBinder);
                    sqlParam.Add(paramBinder, paramVal);
                }
            }

            //var query = "insert into " + table + "(" + string.Join(",", queryColumns) + ") values (" + string.Join(",", queryParamBinders) + ")";


            var query = $"insert into {table} ({string.Join(",", queryColumns)}) values ({string.Join(",", queryParamBinders)})";
            Log.Debug(query);
            return Tuple.Create(query, sqlParam);
        }

        internal static Tuple<string, List<DynamicSqlParameter>> BulkInsert<T>(List<T> objList, string? table = null)
        {
            try
            {
                var tableNameT = ReflectionFactory.GetTableAttribute(typeof(T));
                if (string.IsNullOrEmpty(table) && string.IsNullOrEmpty(tableNameT)) throw new ArgumentNullException(nameof(table) + " cannot be null!");
                if (string.IsNullOrEmpty(table)) table = tableNameT;

                var bulkParamList = new List<DynamicSqlParameter>();
                var queryColumns = new List<string>();
                var queryParamBinders = new List<string>();
                var propInfos = ReflectionFactory.GetMappableProperties(typeof(T));
                foreach (var prop in propInfos)
                {
                    var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                    if (customAttribute == null) continue;

                    //Skip if it is Auto Increment field
                    if (prop.GetCustomAttribute<SqlAutoIncrement>() != null) continue;

                    var colName = customAttribute.PropertyName ?? prop.Name;
                    var paramBinder = "@" + colName;

                    queryColumns.Add(colName);
                    queryParamBinders.Add(paramBinder);
                }

                foreach (var obj in objList)
                {
                    var sqlParam = new DynamicSqlParameter();

                    foreach (var prop in propInfos)
                    {

                        var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                        if (customAttribute == null) continue;

                        //Skip if it is Auto Increment field
                        if (prop.GetCustomAttribute<SqlAutoIncrement>() != null) continue;

                        var colName = customAttribute.PropertyName ?? prop.Name;
                        var paramBinder = "@" + colName;

                        var paramVal = prop.GetValue(obj);
                        if (paramVal != null)
                        {

                            sqlParam.Add(paramBinder, paramVal);
                        }
                        else
                        {
                            sqlParam.Add(paramBinder, GetDefaultValue(prop));
                        }

                    }
                    bulkParamList.Add(sqlParam);
                }

                //var query = "insert into " + table + "(" + string.Join(",", queryColumns) + ") values (" + string.Join(",", queryParamBinders) + ")";

                var query = $"insert into {table} ({string.Join(",", queryColumns)}) values ({string.Join(",", queryParamBinders)})";
                Log.Debug(query);
                return Tuple.Create(query, bulkParamList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static dynamic GetDefaultValue(PropertyInfo prop)
        {
            dynamic? defaultVal = null;
            switch (ReflectionFactory.GetTypeName(prop.PropertyType).ToLower())
            {
                case "int32":
                    defaultVal = 0;
                    break;
                case "string":
                    defaultVal = "";
                    break;
                case "int64":
                    defaultVal = 0;
                    break;
                case "double":
                    defaultVal = 0;
                    break;
                case "decimal":
                    defaultVal = 0;
                    break;
                case "datetime":
                    defaultVal = new DateTime();
                    break;
                case "boolean":
                    defaultVal = null;
                    break;
            }
            return defaultVal;
        }

        internal static Tuple<string, DynamicSqlParameter> Update<T>(T obj, string? table = null)
        {
            var tableNameT = ReflectionFactory.GetTableAttribute(typeof(T));
            if (string.IsNullOrEmpty(table) && string.IsNullOrEmpty(tableNameT)) throw new ArgumentNullException(nameof(table) + " cannot be null!");
            if (string.IsNullOrEmpty(table)) table = tableNameT;

            var sqlParam = new DynamicSqlParameter();

            var updateQuery = new List<string>();
            var propInfos = ReflectionFactory.GetMappableProperties(typeof(T));

            var id = 0;
            var primaryKey = "";
            object primaryKeyValue = "";
            foreach (var prop in propInfos)
            {
                var paramVal = prop.GetValue(obj);
                if (paramVal != null)
                {
                    var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                    if (customAttribute == null) continue;
                    var colName = customAttribute.PropertyName ?? prop.Name;
                    if (prop.GetCustomAttribute<SqlPrimaryKey>() != null)
                    {
                        primaryKey = colName;
                        primaryKeyValue = paramVal.ToString();
                        continue;
                    }
                    //if (colName.Equals("id", StringComparison.OrdinalIgnoreCase))
                    //{

                    //    id = (int)paramVal;
                    //    continue;
                    //}

                    var paramBinder = "@" + colName;

                    updateQuery.Add($"{colName}={paramBinder}");
                    sqlParam.Add(paramBinder, paramVal);
                }
            }

            sqlParam.Add("@primaryKey", primaryKeyValue);
            var query = $"update {table} set {string.Join(",", updateQuery)} where {primaryKey}=@primaryKey";
            Log.Debug(query);
            return Tuple.Create(query, sqlParam);
        }

        /// <summary>
        /// QueryWildCardHelper  
        /// </summary>
        /// <param name="colName"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns>Query, ParamName, ParamValue</returns>
        internal static Tuple<string, string, string> QueryWildCardHelper(string colName, string paramName, string paramValue)
        {
            var op = "=";
            if (paramValue.Contains("*"))
            {
                paramValue = paramValue.Replace("*", "%");
                op = "like";
            }

            var query = $"{colName} {op} {paramName}";

            return Tuple.Create(query, paramName, paramValue);
        }


    }


}

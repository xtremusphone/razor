using Razor01.Global;
using Serilog;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace Factory.DB.Model
{

    public interface IDBObjectBase
    {       
        int Id { get; set; }
    }

    public static class DBObjectBaseExt
    {
        private static Dictionary<string, object> GetPrimaryKey<T>(T thisObj, IEnumerable<PropertyInfo> propInfos)
        {
            var primaryKey = new Dictionary<string, object>();

            foreach (var prop in propInfos)
            {
                var paramVal = prop.GetValue(thisObj);
                if (paramVal != null)
                {
                    var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                    if (customAttribute == null) continue;
                    var colName = customAttribute.PropertyName ?? prop.Name;
                    if (prop.GetCustomAttribute<SqlPrimaryKey>() != null)
                    {
                        primaryKey.Add(colName, paramVal);
                        break;
                    }
                }
            }

            return primaryKey;
        }

        public static async Task<bool> Load(this IDBObjectBase thisObj)
        {
            var objType = thisObj.GetType();
            var sqlParam = new DynamicSqlParameter();

            var propInfos = ReflectionFactory.GetMappableProperties(objType);
            var tableName = ReflectionFactory.GetTableAttribute(objType);

           
            var primaryKey = GetPrimaryKey(thisObj, propInfos).First();

            sqlParam.Add("@primaryKey", primaryKey.Value);
            var query = $"select * from {tableName}  where {primaryKey.Key}=@primaryKey";
            Log.Debug(query);
           
            try
            {
                using (var dbContext = new DBContext(GlobalConfig.Instance.ConnectionString))
                {
                    var dataTable = await dbContext.ExecuteReaderAsync(query, sqlParam);

                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            foreach (var prop in propInfos)
                            {

                                var customAttribute = prop.GetCustomAttribute<SqlPropertyAttribute>();
                                if (customAttribute == null) continue;
                                var colName = customAttribute.PropertyName ?? prop.Name;
                                if (dataRow[colName] == DBNull.Value) continue;


                                if (prop.PropertyType == typeof(DateTime?))
                                {
                                    prop.SetValue(thisObj, dataRow[colName], null);
                                }
                                else
                                {
                                    var val = dataRow[colName].GetType() == prop.PropertyType ? dataRow[colName] : Convert.ChangeType(dataRow[colName], prop.PropertyType);
                                    prop.SetValue(thisObj, val, null);
                                }

                            }

                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                  
                }
               
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static async Task<int> Save(this IDBObjectBase thisObj, bool autoUpdate = true)
        {
            var objType = thisObj.GetType();
            var result = 0;
            var sqlParam = new DynamicSqlParameter();

            var propInfos = ReflectionFactory.GetMappableProperties(objType);
            var tableName = ReflectionFactory.GetTableAttribute(objType);
            var primaryKey = GetPrimaryKey(thisObj, propInfos).First();

            sqlParam.Add("@primaryKey", primaryKey.Value);

            var query = $"select count(*) from {tableName}  where {primaryKey.Key}=@primaryKey";

            try
            {
                using (var dbContext = new DBContext(GlobalConfig.Instance.ConnectionString))
                {
                    var count = (int) Convert.ChangeType(await dbContext.ExecuteScalarAsync(query, sqlParam), typeof(int));

                    if (count > 0)
                    {
                        if (autoUpdate)
                        {
                            //Update
                            var updateQuery = dbContext.QueryFactory.Update(thisObj);
                            result = await dbContext.ExecuteNonQueryAsync(updateQuery.Item1, updateQuery.Item2);
                        }
                        //throw new Exception($"{primaryKey.Key} with values {primaryKey.Value} already exists in {tableName}!");
                    }
                    else
                    {
                        //insert
                        var insertQuery = dbContext.QueryFactory.Insert(thisObj);
                        result = await dbContext.ExecuteNonQueryAsync(insertQuery.Item1, insertQuery.Item2);
                    }
                }

                return result;

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                throw new Exception(ex.Message);
            }
        }

       
    }
}

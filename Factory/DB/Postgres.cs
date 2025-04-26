using Serilog;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Factory.DB
{
    public class PostgresFactory : IDatabaseService
    {
        private bool disposedValue;

        private string _connectionString = string.Empty;
        public string ConnectionString { get { return _connectionString; } set { _connectionString = value; } }

        public PostgresFactory(string constr)
        {
            _connectionString = constr;
        }

        public NpgsqlConnection GetSqlConnection()
        {
            try
            {
                var _connection = NpgsqlDataSource.Create(_connectionString).OpenConnection();
                return _connection;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);

                Log.Error("{funcName}: Connection to PostgresSQL Failed: {error}", funcName, ex.Message);
                throw new Exception(ex.Message);
            }

        }



        public async Task<object> ExecuteScalarAsync(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null)
                    {
                        foreach (NpgsqlParameter parameter in param.Get())
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }

                    var result = await cmd.ExecuteScalarAsync();
                    return result != DBNull.Value ? result : "";
                }
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null)
                    {
                        foreach (NpgsqlParameter parameter in param.Get())
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }

                    return await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        /// <summary>
        /// ExecuteNonQuery_HP_Async - High Performance Execute
        /// </summary>
        /// <param name="query"></param>
        /// <param name="param">DynamicSqlParameter</param>
        /// <returns>Int</returns>
        /// <exception cref="SqlException"></exception>
        public async Task<int> ExecuteNonQuery_HP_Async(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                using (var _connection = GetSqlConnection())
                {

                    using (var command = new NpgsqlCommand("PRAGMA journal_mode = WAL;", _connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var cmd = _connection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        if (param != null)
                        {
                            foreach (NpgsqlParameter parameter in param.Get())
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public async Task<DataTable> ExecuteReaderAsync(string query, DynamicSqlParameter? param = null)
        {

            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null)
                    {
                        foreach (NpgsqlParameter parameter in param.Get())
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }

                    var dataReader = await cmd.ExecuteReaderAsync();
                    var dt = new DataTable();

                    // Loop through each column in the DataReader and add columns to DataTable
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        dt.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                    }

                    // Load data into the DataTable
                    while (await dataReader.ReadAsync())
                    {
                        DataRow row = dt.NewRow();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            row[i] = dataReader.IsDBNull(i) ? DBNull.Value : dataReader.GetValue(i);
                        }
                        dt.Rows.Add(row);
                    }

                    return dt;
                }
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public async Task<DataSet> GetDataSetAsync(string query, DynamicSqlParameter? param = null)
        {

            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null)
                    {
                        foreach (NpgsqlParameter parameter in param.Get())
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }

                    var sda = new NpgsqlDataAdapter(cmd);
                    var ds = new DataSet();
                    sda.Fill(ds);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public async Task<List<T>> QueryAsync<T>(string query, DynamicSqlParameter? param = null) where T : new()
        {
            if (!typeof(T).IsClass)
            {
                throw new ArgumentException("'T' must be a class");
            }


            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;

                    var dataReader = await cmd.ExecuteReaderAsync();
                    return dataReader.MapPropertyAttribute<T>();
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public async Task<JArray> QueryAsJsonArrayAsync(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    var dataReader = await cmd.ExecuteReaderAsync();
                    return dataReader.MapToJsonArray();
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public async Task<List<object>> QueryAsDataListAsync(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null) 
                    {
                        foreach(NpgsqlParameter parameter in param.Get())
                        {
                            cmd.Parameters.Add(parameter);
                        }
                    }

                    var dataReader = await cmd.ExecuteReaderAsync();
                    return dataReader.ToDataList();
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        /// <summary>
        /// InsertNewObjectAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="table"></param>
        /// <returns>The row ID of insert record</returns>
        /// <exception cref="SqlException"></exception>
        public async Task<long> InsertNewObjectAsync<T>(T obj, string? table = null)
        {
            var result = QueryFactory.Insert(obj, table);
            var query = $"{result.Item1};SELECT LAST_INSERT_ID();";
            try
            {
                var insertResult = await ExecuteScalarAsync(query, result.Item2);
                if (insertResult != "")
                    return (long)(ulong)insertResult;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query);
            }
        }

        public void BulkInsert(string query, List<DynamicSqlParameter> bulkList)
        {
            Log.Information($"{new StackFrame().GetMethod().DeclaringType.FullName} : {MethodBase.GetCurrentMethod().Name}");
            try
            {
                using (var _connection = GetSqlConnection())
                {
                    using (var command = new NpgsqlCommand("PRAGMA journal_mode = WAL;", _connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (var trans = _connection.BeginTransaction())
                    {
                        using (var myCmd = new NpgsqlCommand(query, _connection, trans))
                        {
                            myCmd.CommandType = CommandType.Text;

                            foreach (var dynamicSQlParams in bulkList)
                            {
                                myCmd.Parameters.Clear();
                                myCmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                    }
                    stopwatch.Stop();

                    TimeSpan elapsedTime = stopwatch.Elapsed;

                    Log.Information("Processing time: " + elapsedTime);
                    string formattedTime = $"{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds / 10:00}";
                    Log.Information("Processing time (formatted): " + formattedTime);
                }
            }
            catch (Exception ex)
            {
                var funcName = $"{new StackFrame().GetMethod().DeclaringType.FullName} : {MethodBase.GetCurrentMethod().Name}";
                throw new SqlException(ex.Message, ex, funcName, query);
            }

        }

        public void BulkInsert<T>(T obj, List<List<DynamicSqlParameter>> bulkList)
        {
            var query = QueryFactory.Insert(obj);

            try
            {
                BulkInsert(query.Item1, bulkList);
            }
            catch (Exception ex)
            {
                var funcName = $"{new StackFrame().GetMethod().DeclaringType.FullName} : {MethodBase.GetCurrentMethod().Name}";
                throw new SqlException(ex.Message, ex, funcName, query.Item1);
            }
        }

        public void BulkInsert<T>(List<T> obj, string? table = null)
        {
            //Log.Information($"{new StackFrame().GetMethod().DeclaringType.FullName} : {MethodBase.GetCurrentMethod().Name}");

            try
            {
                var query = QueryFactory.BulkInsert(obj, table);

                BulkInsert(query.Item1, query.Item2);
            }
            catch (Exception ex)
            {
                //var funcName = $"{new StackFrame().GetMethod().DeclaringType.FullName} : {MethodBase.GetCurrentMethod().Name}";

                throw;
            }
        }

        public async Task<int> UpdateObjectAsync<T>(T obj, string? table = null)
        {
            var result = QueryFactory.Update(obj, table);
            return await ExecuteNonQueryAsync(result.Item1, result.Item2);
        }

        public async Task<int> CreateTableAsync(Type type, string? table = null)
        {
            string query = QueryFactory.CreateTable(type, table);
            return await ExecuteNonQueryAsync(query);
        }

        public async Task<int> CreateDatabaseAsync(string databaseName)
        {
            var query = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`;";
            return await ExecuteNonQueryAsync(query);
        }

        public async Task<List<T>> ReadMapperAsync<T>(string query, DynamicSqlParameter? param = null) where T : new()
        {
            if (!typeof(T).IsClass)
            {
                throw new ArgumentException("'T' must be a class");
            }

            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;

                    var dataReader = await cmd.ExecuteReaderAsync();
                    //if (typeof(T).GetCustomAttributes<SqlitePropertyAttribute>().Any())
                    return dataReader.MapPropertyAttribute<T>();
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



}

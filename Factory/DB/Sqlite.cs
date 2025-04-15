using Serilog;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;
using Factory.DB;

namespace Factory.DB
{
    public class SQLiteFactory : IDatabaseService, IDisposable
    {
        private bool disposedValue;

        private string _connectionString = string.Empty;

        //To allow different connection string
        public string ConnectionString { get { return _connectionString; } set { _connectionString = value; } }


        public SQLiteFactory(string constr)
        {
            //Set default connection string
            _connectionString = constr;
        }

        public SQLiteConnection GetSqlConnection()
        {
            //Log.Debug("GetSqlConnection: " + _connectionString);
            var _connection = new SQLiteConnection(_connectionString);
           
            try
            {
                _connection.Open();
                return _connection;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);

                Log.Error("{funcName}: Connection to SQLITE Failed: {error}", funcName, ex.Message);
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
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

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
            //Log.Debug("ExecuteNonQueryAsync: " + query);
            try
            {
                //Log.Debug("Before: GetSqlConnection");
                using (var _connection = GetSqlConnection())
                {
                    //Log.Debug(_connection.ConnectionString);
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

                    return await cmd.ExecuteNonQueryAsync();
                }

            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                //Log.Error(query);
                //if (param != null)
                //{
                //    Log.Error("Param");
                //}
                //Log.Error(ex.Message);
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

                    using (var command = new SQLiteCommand("PRAGMA journal_mode = WAL;", _connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var cmd = _connection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        if (param != null) cmd.SetParameters(param.SQLiteParameters);

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
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

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
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

                    var sda = new SQLiteDataAdapter(cmd);
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
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

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

        public async Task<JArray> QueryAsJsonArrayAsync(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                using (var _connection = GetSqlConnection())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.CommandText = query;
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

                    var dataReader = await cmd.ExecuteReaderAsync();
                    //if (typeof(T).GetCustomAttributes<SqlitePropertyAttribute>().Any())
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
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

                    var dataReader = await cmd.ExecuteReaderAsync();
                    //if (typeof(T).GetCustomAttributes<SqlitePropertyAttribute>().Any())
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
                    //using (var command = new SQLiteCommand("PRAGMA synchronous = NORMAL;", _connection))
                    //{
                    //    command.ExecuteNonQuery();
                    //}

                    using (var command = new SQLiteCommand("PRAGMA journal_mode = WAL;", _connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (var trans = _connection.BeginTransaction())
                    {
                        using (var myCmd = new SQLiteCommand(query, _connection, trans))
                        {
                            myCmd.CommandType = CommandType.Text;

                            foreach (var dynamicSQlParams in bulkList)
                            {
                                //Log.Debug(dynamicSQlParams.GetAsString());
                                myCmd.Parameters.Clear();
                                myCmd.SetParameters(dynamicSQlParams.SQLiteParameters);
                                myCmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                        }
                    }
                    stopwatch.Stop();

                    // Get the elapsed time
                    TimeSpan elapsedTime = stopwatch.Elapsed;

                    // Log the processing time
                    //Console.WriteLine("Processing time: " + elapsedTime);
                    Log.Information("Processing time: " + elapsedTime);
                    // Optional: Log the processing time in a specific format
                    string formattedTime = $"{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds / 10:00}";
                    //Console.WriteLine("Processing time (formatted): " + formattedTime);
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
            //Log.Information($"{new StackFrame().GetMethod().DeclaringType.FullName} : {MethodBase.GetCurrentMethod().Name}");
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
                    if (param != null) cmd.SetParameters(param.SQLiteParameters);

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

        //protected void CloseConnection()
        //{
        //    if (_connection != null)
        //    {
        //        if (_connection.State == System.Data.ConnectionState.Open)
        //            _connection.Close();
        //    }

        //    _connection.Dispose();
        //    _connection = null;
        //}

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
                //CloseConnection();
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MSSql()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



}


using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;

namespace Factory.DB
{
    internal class SqliteMemory : IDisposable
    {
        internal SQLiteConnection _connection;

        const string connectionString = "Data Source=:memory:;Version=3;";
        private bool disposedValue;

        internal SqliteMemory()
        {
            GetConnection();
        }

        internal void GetConnection()
        {
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

        public async Task<object> ExecuteScalarAsync(string query, DynamicSqlParameter? param = null)
        {
            try
            {
                var cmd = _connection.CreateCommand();
                cmd.CommandText = query;
                if (param != null) cmd.SetParameters(param.SQLiteParameters);

                var result = await cmd.ExecuteScalarAsync();
                return result != DBNull.Value ? result : "";
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

                var cmd = _connection.CreateCommand();
                cmd.CommandText = query;
                if (param != null) cmd.SetParameters(param.SQLiteParameters);

                return await cmd.ExecuteNonQueryAsync();

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
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw new SqlException(ex.Message, ex, funcName, query, param);
            }
        }

        public DataTable ExecuteReader(string query, DynamicSqlParameter? param = null)
        {

            try
            {

                var cmd = _connection.CreateCommand();
                cmd.CommandText = query;
                if (param != null) cmd.SetParameters(param.SQLiteParameters);

                var dataReader = cmd.ExecuteReader();
                var dt = new DataTable();

                // Loop through each column in the DataReader and add columns to DataTable
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    dt.Columns.Add(dataReader.GetName(i), dataReader.GetFieldType(i));
                }

                // Load data into the DataTable
                while (dataReader.Read())
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
                    if (_connection.State == ConnectionState.Open)
                        _connection.Close();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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

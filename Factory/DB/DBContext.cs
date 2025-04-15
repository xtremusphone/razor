using System.Data;

namespace Factory.DB
{
    public class DBContext : IDatabaseService, IDisposable
    {
     
        public enum DBType
        {
            SQLITE,
            MYSQL
        }
        private bool disposedValue;

        //public Lazy<SQLiteFactory> SQLiteDBInstance => new Lazy<SQLiteFactory>(() => GetSQLiteDBInstance());

        private dynamic DBService;
        public IQueryFactory QueryFactory;
        private string ConnString;

        public DBContext(string connString)
        {
            ConnString = connString;
            DBService = new SQLiteFactory(ConnString);
            QueryFactory = new SqliteQueryFactory();

        }



        public Task<object> ExecuteScalarAsync(string query, DynamicSqlParameter? param = null)
        {
            return DBService.ExecuteScalarAsync(query, param);
        }

        public Task<int> ExecuteNonQueryAsync(string query, DynamicSqlParameter? param = null)
        {
            return DBService.ExecuteNonQueryAsync(query, param);
        }

        public Task<int> ExecuteNonQuery_HP_Async(string query, DynamicSqlParameter? param = null)
        {
            return DBService.ExecuteNonQuery_HP_Async(query, param);
        }

        public Task<DataTable> ExecuteReaderAsync(string query, DynamicSqlParameter? param = null)
        {
            return DBService.ExecuteReaderAsync(query, param);
        }

        public Task<DataSet> GetDataSetAsync(string query, DynamicSqlParameter? param = null)
        {
            return DBService.GetDataSetAsync(query, param);
        }

        public Task<long> InsertNewObjectAsync<T>(T obj, string? table = null)
        {
            return DBService.InsertNewObjectAsync(obj, table);
        }

        public Task<int> UpdateObjectAsync<T>(T obj, string? table = null)
        {
            return DBService.UpdateObjectAsync(obj, table);
        }
        public Task<int> CreateTableAsync(Type type, string? table = null)
        {
            return DBService.CreateTableAsync(type, table);
        }
        public Task<int> CreateDatabaseAsync(string databaseName)
        {
            return DBService.CreateDatabaseAsync(databaseName);
        }

        public void BulkInsert(string query, List<DynamicSqlParameter> bulkList)
        {
            DBService.BulkInsert(query, bulkList);
        }
        public void BulkInsert<T>(T obj, List<List<DynamicSqlParameter>> bulkList)
        {
            DBService.BulkInsert(obj, bulkList);
        }

        public void BulkInsert<T>(List<T> obj, string? table = null)
        {
            DBService.BulkInsert(obj, table);
        }

        
        public Task<List<T>> ReadMapperAsync<T>(string query, DynamicSqlParameter? param = null) where T : new()
        {
            return DBService.ReadMapperAsync<T>(query, param);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                    //SQLiteDBInstance.Value.Dispose();
                    DBService.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DBContext()
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

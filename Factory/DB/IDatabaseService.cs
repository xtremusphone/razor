using Newtonsoft.Json.Linq;
using Factory.DB;
using System.Data;

public interface IDatabaseService
{

    Task<object> ExecuteScalarAsync(string query, DynamicSqlParameter? param = null);
    Task<int> ExecuteNonQueryAsync(string query, DynamicSqlParameter? param = null);
    Task<int> ExecuteNonQuery_HP_Async(string query, DynamicSqlParameter? param = null);
    Task<DataTable> ExecuteReaderAsync(string query, DynamicSqlParameter? param = null);
    Task<DataSet> GetDataSetAsync(string query, DynamicSqlParameter? param = null);
    //Task<List<T>> ReadMapperAsync<T>(string query, DynamicSqlParameter? param = null) where T : new();
    //Task<JArray> Read2JsonArrayAsync(string query, DynamicSqlParameter? param = null);
    //Task<List<object>> Read2ListAsync(string query, DynamicSqlParameter? param = null);
    Task<long> InsertNewObjectAsync<T>(T obj, string? table = null);
    Task<int> UpdateObjectAsync<T>(T obj, string? table = null);
    Task<int> CreateTableAsync(Type type, string? table = null);
    Task<int> CreateDatabaseAsync(string databaseName);
    void BulkInsert(string query, List<DynamicSqlParameter> bulkList);
    void BulkInsert<T>(T obj, List<List<DynamicSqlParameter>> bulkList);
    void BulkInsert<T>(List<T> obj, string? table = null);
    Task<List<T>> ReadMapperAsync<T>(string query, DynamicSqlParameter? param = null) where T : new();
}
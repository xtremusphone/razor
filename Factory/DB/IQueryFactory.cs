
using Factory.DB.Model;



namespace Factory.DB
{
    public interface IQueryFactory
    {
        string CreateTable(Type type, string? table = null);
      
        string TruncateTable(string table);
        string DropTable(string table);
        Tuple<string, DynamicSqlParameter> SimpleQuery(string columnName, string tableName, SqlCondition condition);
        Tuple<string, DynamicSqlParameter> Delete<T>(T obj);
        Tuple<string, DynamicSqlParameter> Insert<T>(T obj, string? table = null);
        Tuple<string, List<DynamicSqlParameter>> BulkInsert<T>(List<T> objList, string? table = null);
        Tuple<string, DynamicSqlParameter> Update<T>(T obj, string? table = null);
        Tuple<string, string, string> QueryWildCardHelper(string colName, string paramName, string paramValue);
        Tuple<string, DynamicSqlParameter> IsTableExists(string table);
        Tuple<string, DynamicSqlParameter> SearchTables(string table);
    }
}

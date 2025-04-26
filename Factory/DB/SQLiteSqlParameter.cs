
using System.Data.Common;
using System.Data.SQLite;


namespace Factory.DB
{
    public class SQLiteSqlParameter : ISQLParameter
    {
        private List<Tuple<string, object>> sqlParams;

        public SQLiteSqlParameter()
        {
            sqlParams = new List<Tuple<string, object>>();
        }

        public DbParameter[] Get()
        {
            var sqlParameter = new List<SQLiteParameter>();
            foreach (var param in sqlParams)
            {
                sqlParameter.Add(new SQLiteParameter(param.Item1, param.Item2));
            }
            return sqlParameter.ToArray();
        }

        public void Add(string name, object value)
        {
            sqlParams.Add(Tuple.Create(name, value));

        }

        public string GetAsString()
        {
            return string.Join(',', sqlParams.Select(tuple => $"{tuple.Item1}: {tuple.Item2}"));
        }

        public void Clear()
        {
            sqlParams.Clear();
        }
    }
}

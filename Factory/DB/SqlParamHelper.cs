
using System.Data.SQLite;


namespace Factory.DB
{
    public class DynamicSqlParameter
    {

        private List<Tuple<string, object>> sqlParams;
        public DynamicSqlParameter()
        {
            sqlParams = new List<Tuple<string, object>>();
        }

        public SQLiteParameter[] SQLiteParameters
        {
            get
            {
                var sqlParameter = new List<SQLiteParameter>();
                foreach (var param in sqlParams)
                {
                    sqlParameter.Add(new SQLiteParameter(param.Item1, param.Item2));
                }
                return sqlParameter.ToArray();

            }
        }

        

        public void Add(string name, object value)
        {
            sqlParams.Add(Tuple.Create(name, value));

        }


        public string GetAsString()
        {
            return string.Join(',', sqlParams.Select(tuple => $"{tuple.Item1}: {tuple.Item2}"));
        }
    }
}

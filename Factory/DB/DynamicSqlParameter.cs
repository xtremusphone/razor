using System.Data;
using System.Data.Common;
using static Factory.DB.DBContext;

namespace Factory.DB
{
    public class DynamicSqlParameter
    {
        private ISQLParameter sqlParams;

        public DynamicSqlParameter(DBType type)
        {
            switch (type)
            {
                case DBType.SQLITE:
                    sqlParams = new SQLiteSqlParameter();
                    break;
                default:
                    sqlParams = new PostgresSqlParameter();
                    break;
            }
        }

        public DbParameter[] Get()
        {
            return sqlParams.Get();
        }

        public void Add(string name, object value)
        {
            sqlParams.Add(name,value);
        }

        public string GetAsString()
        {
            return sqlParams.GetAsString();
        }

        public void Clear()
        {
            
        }
    }
}
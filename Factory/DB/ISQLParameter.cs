using System.Data.Common;

namespace Factory.DB {
    public interface ISQLParameter {
        public DbParameter[] Get();
        public void Add(string name, object value);
        public string GetAsString();
        public void Clear();
    }
}
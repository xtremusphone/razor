namespace Factory.DB
{
    public enum DataType
    {
        INT,
        VARCHAR,
        TEXT,
        BIGINT,
        DOUBLE,
        DECIMAL,
        FLOAT,
        DATETIME,
        BOOL
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class SqlPropertyAttribute : Attribute
    {
        public string PropertyName { get; private set; }
        public string DataType { get; private set; }
        public int DataSize { get; private set; }
        public SqlPropertyAttribute(string name, DataType dataType)
        {
            PropertyName = name;
            DataType = dataType.ToString();

        }

        public SqlPropertyAttribute(string name, DataType dataType, int dataSize)
        {
            PropertyName = name;
            DataType = dataType.ToString();
            DataSize = dataSize;
        }

        public SqlPropertyAttribute(string name)
        {
            PropertyName = name;
            DataType = string.Empty;
        }
    }
    public class SqlTableAttribute : Attribute
    {
        public string PropertyName { get; private set; }

        public SqlTableAttribute(string name)
        {
            PropertyName = name;
        }
    }
    public class SqlPrimaryKey : Attribute
    {
        public SqlPrimaryKey()
        {
        }
    }
    public class SqlAutoIncrement : Attribute
    {
        public SqlAutoIncrement()
        {
        }
    }
}

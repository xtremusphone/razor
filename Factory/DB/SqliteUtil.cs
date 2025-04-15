using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.DB
{
    internal struct SqliteUtil
    {
        public const string PrimaryKey = "PRIMARY KEY";
        public const string Autoincrement = "AUTOINCREMENT";

        public static string GetSqliteDataType(string dataTypeStr)
        {
            var dataType = "";

            switch (dataTypeStr.ToLower())
            {
                case "int":
                case "int32":
                    dataType = "INTEGER";
                    break;
                case "varchar":
                case "string":
                    dataType = "VARCHAR";
                    break;
                case "text":
                    dataType = "TEXT";
                    break;
                case "bigint":
                    dataType = "BIGINT";
                    break;
                case "double":
                    dataType = "DOUBLE";
                    break;
                case "decimal":
                    dataType = "DECIMAL";
                    break;
                case "datetime":
                    dataType = "DATETIME";
                    break;
                case "boolean":
                    dataType = "BOOL";
                    break;
            }
            return dataType;
        }

        public static string GetSqliteMemoryConnectionString()
        {
            return "Data Source=:memory:;Version=3;";
        }
    }
}

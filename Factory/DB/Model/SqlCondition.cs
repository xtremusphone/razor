using static Factory.DB.DBContext;

namespace Factory.DB.Model
{

    /// <summary>
    /// SQlCondition helper - Default is SQLITE format, use SetlaceholderSign to change to MySQL compatible
    /// </summary>
    public class SqlCondition
    {
        private string _placeholder_sign = "@"; //sqlite
        public string ParamName { get; set; }
        public QueryOperator Operator { get; set; }
        public string ParamPlaceholder { get { return $"{_placeholder_sign}{ParamName}"; } }
        public string ParamValue { get; set; }


        public SqlCondition()
        {

        }

        public SqlCondition(string paramName, QueryOperator op, string paramValue)
        {
            ParamName = paramName;
            Operator = op;
            ParamValue = paramValue;
        }

        public void SetPlaceholderSign(DBType dbType)
        {
            if (dbType == DBType.SQLITE)
            {
                _placeholder_sign = "@";//sqlite
            }
            else
            {
                _placeholder_sign = "$";
            }
        }

        public override string ToString()
        {
            return $"{ParamName} {GetOperator(Operator)} {ParamPlaceholder}";
        }

        public string GetOperator(QueryOperator op)
        {
            var opStr = "=";
            switch (op)
            {
                case QueryOperator.Equal: opStr = "="; break;
                case QueryOperator.Greater: opStr = ">"; break;
                case QueryOperator.GreaterOrEqual: opStr = ">="; break;
                case QueryOperator.Less: opStr = "<"; break;
                case QueryOperator.LessOrEqual: opStr = "<="; break;
                case QueryOperator.Like: opStr = "like"; break;
            }
            return opStr;
        }
    }

    public enum QueryOperator
    {
        Equal,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        Like
    }
}

namespace Factory.DB
{
    internal class QueryFactoryUtil
    {
    }

    public struct QueryParam
    {
        public string ParamName { get; set; }
        public Type ParamType { get; set; }
        public QueryOperator QueryOperator { get; set; }
        public string ParamValue { get; set; }

    }

    public struct QueryOperator
    {
        public string Equal => "=";
        public string Greater => ">";
        public string GreaterOrEqual => ">=";
        public string Less => "<";
        public string LessOrEqual => "<=";
        public string Like => "like";
    }
}

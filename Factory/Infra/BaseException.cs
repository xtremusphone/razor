using System.Runtime.Serialization;

namespace Factory.Infra
{
    [Serializable]
    public class BaseException : Exception
    {
        public BaseException()
        {
        }

        public BaseException(string message)
        {
        }

        public BaseException(string message, Exception innerException) { }

        protected BaseException(SerializationInfo info, StreamingContext context) { }

        public BaseException(string funcName, string message) : base(string.Format("Function : {0} => Exception: {1}", funcName, message))
        {

        }
    }
}

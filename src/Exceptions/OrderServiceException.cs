using System.Runtime.Serialization;

namespace OrderRice.Exceptions
{
    public class OrderServiceException : Exception
    {
        public OrderServiceException()
        {
        }

        public OrderServiceException(string message) : base(message)
        {
        }

        public OrderServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OrderServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

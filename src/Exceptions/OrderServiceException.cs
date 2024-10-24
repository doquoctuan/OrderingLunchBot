using System.Runtime.Serialization;

namespace OrderLunch.Exceptions
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
    }
}

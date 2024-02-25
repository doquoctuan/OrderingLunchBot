using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

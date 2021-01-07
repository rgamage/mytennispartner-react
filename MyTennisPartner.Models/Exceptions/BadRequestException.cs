using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.Exceptions
{
    /// <summary>
    /// Exception to be used when invalid data is detected within a database access procedure or some other non-controller method.
    /// When a controller detects this kind of exception, it should pass a 400 BadRequest to the client, with the message in this exception
    /// </summary>
    public class BadRequestException: Exception
    {
        public BadRequestException()
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception to be used when invalid data is detected within a database access procedure or some other non-controller method.
    /// When a controller detects this kind of exception, it should pass a 404 Not Found to the client, with the message in this exception
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }


}

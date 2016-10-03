using System;

namespace Hornbill
{
    /// <summary>
    /// The exception that is thrown when an error occurs while accessing the network through a pluggable protocol.
    /// </summary>
    public class RequestFailureException : System.Net.WebException
    {
        ///<summary>Initializes a new instance of the RequestFailureException class.</summary> 
        public RequestFailureException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Net.WebException class with the specified error message.
        /// </summary>
        /// <param name="message">The text of the error message.</param>
        public RequestFailureException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RequestFailureException class with the specified error message and nested exception.
        /// </summary>
        /// <param name="message">The text of the error message.</param>
        /// <param name="inner">A nested exception.</param>
        public RequestFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

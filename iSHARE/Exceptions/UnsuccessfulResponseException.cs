using System;

namespace iSHARE.Exceptions
{
    public class UnsuccessfulResponseException : Exception
    {
        public UnsuccessfulResponseException(string message) : base(message)
        {
        }

        public UnsuccessfulResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

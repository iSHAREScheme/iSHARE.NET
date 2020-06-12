using System;

namespace iSHARE.Internals
{
    internal class TokenNotFoundException : Exception
    {
        public TokenNotFoundException(string message) : base(message)
        {
        }
    }
}

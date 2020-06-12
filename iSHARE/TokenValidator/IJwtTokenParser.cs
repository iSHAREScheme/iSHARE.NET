using System;
using iSHARE.Exceptions;
using iSHARE.TokenValidator.Models;

namespace iSHARE.TokenValidator
{
    public interface IJwtTokenParser
    {
        /// <summary>
        /// Parses a string representation of a signed JWT token into actual token.
        /// </summary>
        /// <param name="jwtToken">String representation of a signed JWT token.</param>
        /// <returns>Model which encapsulates the parsed data.</returns>
        /// <exception cref="ArgumentNullException">Throws if invalid argument is passed.</exception>
        /// <exception cref="InvalidTokenException">Throws if JWT token is invalid or not signed (x5c header doesn't exist).</exception>
        AssertionModel Parse(string jwtToken);
    }
}

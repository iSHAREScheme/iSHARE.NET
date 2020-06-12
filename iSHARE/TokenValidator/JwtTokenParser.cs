using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using iSHARE.Exceptions;
using iSHARE.TokenValidator.Models;

namespace iSHARE.TokenValidator
{
    internal class JwtTokenParser : IJwtTokenParser
    {
        public AssertionModel Parse(string jwtToken)
        {
            ValidateArgument(jwtToken);

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
                EnsureTokenIsSigned(jwtSecurityToken);

                var certificates = jwtSecurityToken.Header["x5c"].ToString();
                var chain = JsonSerializer.Deserialize<string[]>(certificates);

                return new AssertionModel(chain, jwtSecurityToken, jwtToken);
            }
            catch (InvalidTokenException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidTokenException("Couldn't parse the token, because it's invalid.", e);
            }
        }

        private static void ValidateArgument(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                throw new ArgumentNullException(nameof(jwtToken), "JWT token value must be present.");
            }
        }

        private static void EnsureTokenIsSigned(JwtSecurityToken jwtSecurityToken)
        {
            if (!jwtSecurityToken.Header.ContainsKey("x5c"))
            {
                throw new InvalidTokenException("JWT token does not contain any certificates (x5c header).");
            }
        }
    }
}

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace iSHARE
{
    public static class TokenConvert
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Deserializes JWT payload into object.
        /// </summary>
        /// <typeparam name="T">Type to which deserialization should be applied.</typeparam>
        /// <param name="jwtToken">JWT token which contains claim that should be deserialized.</param>
        /// <param name="claimName">
        /// iSHARE compliant JWTs contain actual response object under specific claim name. 
        /// To do a deserialization it is needed to know the claim name which value is going to be deserialized. 
        /// E.x. 'delegation_token'.
        /// </param>
        /// <returns>Deserialized object of type <see cref="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Throws if invalid arguments are passed.</exception>
        /// <exception cref="InvalidOperationException">Throws if <see cref="claimName"/> does not exist in <see cref="jwtToken"/>.</exception>
        public static T DeserializeClaim<T>(JwtSecurityToken jwtToken, string claimName)
        {
            ValidateArguments(jwtToken, claimName);

            var claims = jwtToken.Claims.Where(c => c.Type == claimName).ToArray();
            if (claims.Length == 0)
            {
                throw new InvalidOperationException($"Claim {claimName} does not exist.");
            }

            return JsonSerializer.Deserialize<T>(
                claims.Length == 1
                    ? claims.First().Value
                    : BuildArrayString<T>(claims),
                Options);
        }

        private static string BuildArrayString<T>(Claim[] claims)
        {
            return $"[{string.Join(",", claims.Select(x => x.Value))}]";
        }

        private static void ValidateArguments(JwtSecurityToken jwtToken, string claimName)
        {
            if (jwtToken == null)
            {
                throw new ArgumentNullException(nameof(jwtToken));
            }

            if (string.IsNullOrWhiteSpace(claimName))
            {
                throw new ArgumentNullException(nameof(claimName));
            }
        }
    }
}

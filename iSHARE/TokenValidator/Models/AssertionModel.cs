using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace iSHARE.TokenValidator.Models
{
    public class AssertionModel
    {
        public AssertionModel(
            IReadOnlyCollection<string> certificates,
            JwtSecurityToken jwtSecurityToken,
            string jwtToken)
        {
            Certificates = certificates;
            JwtSecurityToken = jwtSecurityToken;
            JwtToken = jwtToken;
        }

        public IReadOnlyCollection<string> Certificates { get; }

        public JwtSecurityToken JwtSecurityToken { get; }

        public string JwtToken { get; }
    }
}

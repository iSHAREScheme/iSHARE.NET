using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using iSHARE.Exceptions;
using iSHARE.Internals;
using iSHARE.TokenValidator.Args;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace iSHARE.TokenValidator
{
    internal class DecodedJwtValidator : IDecodedJwtValidator
    {
        private readonly ILogger<DecodedJwtValidator> _logger;

        public DecodedJwtValidator(ILogger<DecodedJwtValidator> logger)
        {
            _logger = logger;
        }

        public bool IsIShareCompliant(TokenValidationArgs args)
        {
            var keys = SecurityKeysExtractor.Extract(args.AssertionModel.Certificates);
            if (keys.Count == 0)
            {
                return false;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var validationParameters = CreateTokenValidationParameters(args.Issuer, args.Audience, keys);
                handler.ValidateToken(args.AssertionModel.JwtToken, validationParameters, out var securityToken);

                var jwtSecurityToken = (JwtSecurityToken)securityToken;
                if (jwtSecurityToken.RawData != args.AssertionModel.JwtSecurityToken.RawData)
                {
                    _logger.LogWarning("Keys that were used to verify this token might not be a part of this JWT.");
                    return false;
                }

                if (IsHeaderInvalid(jwtSecurityToken.Header))
                {
                    _logger.LogWarning($"Invalid JWT header. Jwt issued by {args.Issuer}.");
                    return false;
                }

                if (IsPayloadInvalid(jwtSecurityToken.Payload))
                {
                    _logger.LogWarning($"Invalid JWT payload. Jwt issued by {args.Issuer}.");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "JWT token is not iSHARE compliant, validation failed.");
                return false;
            }
        }

        private static TokenValidationParameters CreateTokenValidationParameters(
            string issuer,
            string audience,
            IEnumerable<SecurityKey> trustedKeys)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = trustedKeys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,
                ValidateIssuer = true,
                
                RequireSignedTokens = true,
                RequireExpirationTime = true,

                ValidateAudience = false
            };

            if (audience != null)
            {
                tokenValidationParameters.ValidAudience = audience;
                tokenValidationParameters.ValidateAudience = true;
            }

            return tokenValidationParameters;
        }

        private static bool IsHeaderInvalid(JwtHeader header)
        {
            static bool IsAlgInvalid(string alg) => alg != SecurityAlgorithms.RsaSha256;
            static bool IsTypInvalid(string typ) => typ != "JWT";

            return IsAlgInvalid(header.Alg) || IsTypInvalid(header.Typ);
        }

        private static bool IsPayloadInvalid(JwtPayload payload)
        {
            static bool IsSubInvalid(string sub, string iss) => sub != iss;
            static bool IsJtiInvalid(string jti) => string.IsNullOrWhiteSpace(jti);
            static bool WasIssuedBeforeNow(int? iat) => iat == null || DateTime.UtcNow.ToEpochTime() < iat;
            static bool HasIncorrectExpiration(int? iat, int? exp) => exp - iat != 30;

            return IsSubInvalid(payload.Sub, payload.Iss)
               || IsJtiInvalid(payload.Jti)
               || WasIssuedBeforeNow(payload.Iat)
               || HasIncorrectExpiration(payload.Iat, payload.Exp);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using OpenSSL.PrivateKeyDecoder;

namespace iSHARE.IntegrationTests
{
    internal class JwtUtilities
    {
        public static string Create(
            string issuer,
            string audience,
            string privateKeyText,
            string publicKeyBase64Der)
        {
            var token = CreateToken(issuer, audience, publicKeyBase64Der, privateKeyText);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static JwtSecurityToken CreateToken(
            string clientId,
            string audience,
            string publicKeyBase64Der,
            string privateKeyText)
        {
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                clientId,
                audience,
                new List<Claim>
                {
                    new Claim(JwtClaimTypes.Subject, clientId),
                    new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
                    new Claim(JwtClaimTypes.IssuedAt, DateTimeToEpoch(now), ClaimValueTypes.Integer),
                },
                notBefore: null,
                now.AddSeconds(30),
                CreateSigningCredentials(privateKeyText)
            );

            if (publicKeyBase64Der != null)
            {
                token.Header.Add("x5c", new[] { publicKeyBase64Der });
            }

            return token;
        }

        private static SigningCredentials CreateSigningCredentials(string privateKeyText)
        {
            var rsa = StringToRsa(privateKeyText);

            return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        }

        private static RSA StringToRsa(string privateKey)
        {
            var decoder = new OpenSSLPrivateKeyDecoder();
            var rsaParams = decoder.DecodeParameters(privateKey, null);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);

            return rsa;
        }

        public static string DateTimeToEpoch(DateTime value)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var elapsedTime = value - epoch;

            return ((long)elapsedTime.TotalSeconds).ToString(CultureInfo.CurrentCulture);
        }
    }
}
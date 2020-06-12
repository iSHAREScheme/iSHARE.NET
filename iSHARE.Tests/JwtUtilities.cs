using IdentityModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using OpenSSL.PrivateKeyDecoder;

namespace iSHARE.Tests
{
    internal class JwtUtilities
    {
        public static string Create(
            string issuer,
            string audience,
            string privateKeyText,
            string publicKeyBase64Der)
        {
            var token = CreateToken(
                issuer,
                audience,
                publicKeyBase64Der,
                CreateSigningCredentials(privateKeyText));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static JwtSecurityToken CreateToken(
            string clientId,
            string audience,
            string publicKeyBase64Der,
            SigningCredentials signingCredentials)
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
                signingCredentials
            );

            if (publicKeyBase64Der != null)
            {
                token.Header.Add("x5c", new[] { publicKeyBase64Der });
            }

            return token;
        }

        public static SigningCredentials CreateSigningCredentials(
            string privateKeyText,
            string alg = SecurityAlgorithms.RsaSha256)
        {
            var rsa = StringToRsa(privateKeyText);

            return new SigningCredentials(new RsaSecurityKey(rsa), alg);
        }

        public static string SignJwt(string contentJson, string privateKey)
        {
            var jwt = EncodeJwtFromJson(contentJson);
            var signature = Sign(jwt, privateKey);

            return jwt + "." + signature;
        }

        public static string DateTimeToEpoch(DateTime value)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var elapsedTime = value - epoch;

            return ((long)elapsedTime.TotalSeconds).ToString(CultureInfo.CurrentCulture);
        }

        private static string Sign(string data, string privateKey)
        {
            var decoder = new OpenSSLPrivateKeyDecoder();

            var rsaParams = decoder.DecodeParameters(privateKey, null);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);


            var signature = rsa.SignData(Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            return Base64UrlEncode(signature);
        }

        private static string EncodeJwtFromJson(string contentJson)
        {
            var newContent = Regex.Replace(contentJson, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

            var result = new StringBuilder();
            var splitContent = newContent.Split("}.{");
            var header = splitContent[0];
            var payload = splitContent[1];

            result.Append(Base64Encode((header + "}")));
            result.Append(".");
            if (!string.IsNullOrEmpty(payload))
            {
                result.Append(Base64Encode(("{" + payload)));
            }

            return result.ToString();
        }

        private static RSA StringToRsa(string privateKey)
        {
            var decoder = new OpenSSLPrivateKeyDecoder();
            var rsaParams = decoder.DecodeParameters(privateKey, null);
            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);

            return rsa;
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0];
            output = output.Replace('+', '-');
            output = output.Replace('/', '_');
            return output;
        }
        private static string Base64Encode(string content) => Base64UrlEncode(Encoding.UTF8.GetBytes(content));
    }
}
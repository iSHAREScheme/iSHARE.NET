using System;
using iSHARE.TokenValidator.Models;

namespace iSHARE.TokenValidator.Args
{
    public class TokenValidationArgs
    {
        /// <summary>
        /// Constructor for object which is used to encapsulate <see cref="IJwtTokenResponseValidator"/> parameters.
        /// </summary>
        /// <param name="assertionModel">Assertion model which contains JWT contents.</param>
        /// <param name="issuer">Party's EORI which issued the token.</param>
        /// <param name="audience">
        /// Party's EORI to whom token was issued. If not exists, then it won't be validated.
        /// E.x. Some response, like /capabilities can be accessed anonymously, that's why this validation is optional.
        /// </param>
        /// <exception cref="ArgumentNullException">Any of values are null or strings are whitespaces.</exception>
        public TokenValidationArgs(AssertionModel assertionModel, string issuer, string audience = null)
        {
            ValidateArguments(assertionModel, issuer, audience);

            AssertionModel = assertionModel;
            Issuer = issuer;
            Audience = audience;
        }

        public AssertionModel AssertionModel { get; }

        public string Issuer { get; }

        public string Audience { get; }

        private static void ValidateArguments(AssertionModel assertionModel, string issuer, string audience)
        {
            if (assertionModel == null)
            {
                throw new ArgumentNullException(nameof(assertionModel));
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            if (audience != null && string.IsNullOrWhiteSpace(audience))
            {
                throw new ArgumentNullException(nameof(audience), "It cannot be empty or whitespace.");
            }
        }
    }
}

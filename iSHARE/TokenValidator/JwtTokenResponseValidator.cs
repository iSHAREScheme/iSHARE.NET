using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.TokenValidator.Args;
using Microsoft.Extensions.Logging;

namespace iSHARE.TokenValidator
{
    internal class JwtTokenResponseValidator : IJwtTokenResponseValidator
    {
        private readonly IDecodedJwtValidator _decodedJwtValidator;
        private readonly IJwtCertificateValidator _jwtCertificateValidator;
        private readonly ILogger<JwtTokenResponseValidator> _logger;

        public JwtTokenResponseValidator(
            IDecodedJwtValidator decodedJwtValidator,
            IJwtCertificateValidator jwtCertificateValidator,
            ILogger<JwtTokenResponseValidator> logger)
        {
            _decodedJwtValidator = decodedJwtValidator;
            _jwtCertificateValidator = jwtCertificateValidator;
            _logger = logger;
        }

        public async Task<bool> IsValidAsync(
            TokenValidationArgs args,
            string schemeOwnerAccessToken,
            CancellationToken token = default)
        {
            if (!_decodedJwtValidator.IsIShareCompliant(args))
            {
                return false;
            }

            try
            {
                var validationArgs = new CertificateValidationArgs(
                    CertificateUtilities.FromBase64Der(args.AssertionModel.Certificates.First()),
                    args.Issuer,
                    args.AssertionModel.Certificates.Skip(1).Select(CertificateUtilities.FromBase64Der));

                return await _jwtCertificateValidator.IsValidAsync(validationArgs, schemeOwnerAccessToken, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't create proper CertificateValidationArgs. Certificates are corrupted.");

                return false;
            }
        }
    }
}

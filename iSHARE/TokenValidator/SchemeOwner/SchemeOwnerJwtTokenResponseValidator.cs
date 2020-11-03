using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using iSHARE.Internals;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;
using iSHARE.TokenValidator.SchemeOwner.RootCertificate;
using iSHARE.TokenValidator.SchemeOwner.TestCaStrategy;
using Microsoft.Extensions.Logging;

namespace iSHARE.TokenValidator.SchemeOwner
{
    internal class SchemeOwnerJwtTokenResponseValidator : ISchemeOwnerJwtTokenResponseValidator
    {
        private readonly IDecodedJwtValidator _decodedJwtValidator;
        private readonly IRootCertificateStorage _rootCertificateStorage;
        private readonly ITestCaStrategy _testCaStrategy;
        private readonly ILogger<SchemeOwnerJwtTokenResponseValidator> _logger;
        private readonly string _partyId;

        public SchemeOwnerJwtTokenResponseValidator(
            IDecodedJwtValidator decodedJwtValidator,
            IRootCertificateStorage rootCertificateStorage,
            ILogger<SchemeOwnerJwtTokenResponseValidator> logger,
            ITestCaStrategy testCaStrategy,
            IShareSettings settings)
        {
            _decodedJwtValidator = decodedJwtValidator;
            _rootCertificateStorage = rootCertificateStorage;
            _logger = logger;
            _testCaStrategy = testCaStrategy;
            _partyId = settings.Eori;
        }

        public bool IsValid(AssertionModel assertionModel)
        {
            try
            {
                if (!_decodedJwtValidator.IsIShareCompliant(CreateTokenValidationArgs(assertionModel)))
                {
                    return false;
                }

                if (!IsRootCertificateTrusted(CertificateUtilities.FromBase64Der(assertionModel.Certificates.Last())))
                {
                    _logger.LogWarning("SO root certificate is untrusted.");
                    
                    return false;
                }

                var x509Certificate = CertificateUtilities.FromBase64Der(assertionModel.Certificates.First());
                var additionalCertificates = assertionModel.Certificates.Skip(1)
                    .Select(CertificateUtilities.FromBase64Der)
                    .ToArray();

                return IsChainValid(x509Certificate, additionalCertificates) && DoesBelongToSchemeOwner(x509Certificate);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while validating token response retrieved from Scheme Owner.");

                return false;
            }
        }

        private static bool DoesBelongToSchemeOwner(X509Certificate2 certificate) =>
            certificate.SubjectName.Name?.Contains("iSHARE") ?? false;

        private bool IsRootCertificateTrusted(X509Certificate2 certificate) =>
            certificate.GetSha256() == _rootCertificateStorage.GetSha256();

        private TokenValidationArgs CreateTokenValidationArgs(AssertionModel assertionModel) =>
            new TokenValidationArgs(assertionModel, Constants.SchemeOwnerEori, _partyId);

        private bool IsChainValid(X509Certificate2 primaryCertificate, X509Certificate2[] additionalCertificates)
        {
            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.ExtraStore.AddRange(additionalCertificates);

                chain.ChainPolicy.RevocationMode = _testCaStrategy.GetRevocationMode();
                var isValidByPolicy = chain.Build(primaryCertificate);
                if (isValidByPolicy)
                {
                    return true;
                }

                var statuses = chain
                    .ChainElements
                    .OfType<X509ChainElement>()
                    .SelectMany(c => c.ChainElementStatus)
                    .ToList();

                if (_testCaStrategy.ShouldErrorsBeIgnored(statuses))
                {
                    // allow untrusted root
                    // for the places where the iSHARE root is not installed (build server)
                    isValidByPolicy = true;
                }

                _logger.LogInformation(
                    "Chain validation status information {results}.", statuses.Select(c => c.StatusInformation).ToList());

                return isValidByPolicy;
            }
        }
    }
}
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Parties;
using iSHARE.Parties.Args;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.SchemeOwner.TestCaStrategy;
using iSHARE.TrustedList;
using Microsoft.Extensions.Logging;

namespace iSHARE.TokenValidator
{
    internal class JwtCertificateValidator : IJwtCertificateValidator
    {
        private readonly IPartiesQueryService _partiesQueryService;
        private readonly ITrustedListQueryService _trustedListQueryService;
        private readonly ITestCaStrategy _testCaStrategy;
        private readonly ILogger<JwtCertificateValidator> _logger;

        public JwtCertificateValidator(
            IPartiesQueryService partiesQueryService,
            ITrustedListQueryService trustedListQueryService,
            ITestCaStrategy testCaStrategy,
            ILogger<JwtCertificateValidator> logger)
        {
            _partiesQueryService = partiesQueryService;
            _trustedListQueryService = trustedListQueryService;
            _testCaStrategy = testCaStrategy;
            _logger = logger;
        }

        public async Task<bool> IsValidAsync(
            CertificateValidationArgs validationArgs,
            string schemeOwnerAccessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!IsChainValid(validationArgs.PartyCertificate, validationArgs.AdditionalCertificates))
                {
                    return false;
                }

                if (!await DoesCertificateBelongToParty(validationArgs, schemeOwnerAccessToken, cancellationToken))
                {
                    return false;
                }

                return await IsRootCertificateTrusted(
                    validationArgs.AdditionalCertificates.Last(),
                    schemeOwnerAccessToken,
                    cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Error occurred during JWT x5c validation.", e);

                return false;
            }
        }

        private bool IsChainValid(X509Certificate2 primaryCertificate, X509Certificate2[] additionalCertificates)
        {
            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.ExtraStore.AddRange(additionalCertificates);

                chain.ChainPolicy.RevocationMode = _testCaStrategy.GetRevocationMode();
                var isValid = chain.Build(primaryCertificate);
                if (isValid)
                {
                    return true;
                }

                var statuses = chain
                    .ChainElements
                    .OfType<X509ChainElement>()
                    .SelectMany(c => c.ChainElementStatus)
                    .ToArray();

                if (_testCaStrategy.ShouldErrorsBeIgnored(statuses))
                {
                    // allow untrusted root
                    // for the places where the iSHARE root is not installed (build server)
                    // even if it's untrusted, trusted list service will do the last check to assure it's actually trusted
                    isValid = true;
                }

                _logger.LogInformation(
                    "Chain validation status information {results}.", statuses.Select(c => c.StatusInformation).ToList());

                return isValid;
            }
        }

        private async Task<bool> DoesCertificateBelongToParty(
            CertificateValidationArgs validationArgs,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var subjectName = validationArgs.PartyCertificate.SubjectName.Name;
            var args = new PartiesRequestArgs(
                accessToken,
                eori: validationArgs.PartyEori,
                certificateSubjectName: subjectName);

            var response = await _partiesQueryService.GetAsync(args, cancellationToken);
            if (response.Count != 1)
            {
                _logger.LogWarning(
                    "Parties response count for eori: {eori} and subject: {subjectName} was not 1. Count: {count}.",
                    validationArgs.PartyEori,
                    subjectName,
                    response.Count);

                return false;
            }

            var adherence = response.Parties.First().Adherence;

            return adherence.Status == "Active"
               && adherence.StartDate < DateTime.UtcNow
               && (!adherence.EndDate.HasValue || adherence.EndDate > DateTime.UtcNow);
        }

        private async Task<bool> IsRootCertificateTrusted(
            X509Certificate2 certificate,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var trustedList = await _trustedListQueryService.GetAsync(accessToken, cancellationToken);
            var fingerprint = certificate.GetSha256();

            return trustedList.Any(x => x.CertificateFingerprint == fingerprint);
        }
    }
}
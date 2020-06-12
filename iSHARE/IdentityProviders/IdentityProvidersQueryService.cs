using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Capabilities;
using iSHARE.Capabilities.Args;
using iSHARE.Capabilities.Responses;
using iSHARE.Exceptions;
using iSHARE.IdentityProviders.Responses;
using iSHARE.Internals;
using iSHARE.Parties;
using iSHARE.Parties.Args;
using iSHARE.Parties.Responses;
using iSHARE.TokenValidator;
using Microsoft.Extensions.Logging;

namespace iSHARE.IdentityProviders
{
    internal class IdentityProvidersQueryService : IIdentityProvidersQueryService
    {
        private const string IdentityProvider = "IdentityProvider";

        private readonly IPartiesQueryService _partiesQueryService;
        private readonly ICapabilitiesQueryService _capabilitiesQueryService;
        private readonly ILogger<IdentityProvidersQueryService> _logger;

        public IdentityProvidersQueryService(
            IPartiesQueryService partiesQueryService,
            ICapabilitiesQueryService capabilitiesQueryService,
            ILogger<IdentityProvidersQueryService> logger)
        {
            _partiesQueryService = partiesQueryService;
            _capabilitiesQueryService = capabilitiesQueryService;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<IdentityProvider>> GetAsync(string accessToken, CancellationToken token)
        {
            try
            {
                var parties = await RetrieveIdpPartiesAsync(accessToken, token);
                if (parties.Count == 0)
                {
                    throw new UnsuccessfulResponseException("Identity providers were not found.");
                }

                return await RetrieveAuthorizeUrlsAsync(parties, accessToken, token);
            }
            catch (UnsuccessfulResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnsuccessfulResponseException("Identity providers request was unsuccessful.", e);
            }
        }

        private static string FormatIdpUri(string uri)
        {
            return uri.Remove(uri.LastIndexOf("authorize", StringComparison.Ordinal)).RemoveSlashSuffix();
        }

        private async Task<ICollection<Party>> RetrieveIdpPartiesAsync(string accessToken, CancellationToken token)
        {
            static bool IsValidIdp(Party party)
            {
                var certification = party.Certifications.FirstOrDefault(c => c.Role == IdentityProvider);
                if (certification == null)
                {
                    return false;
                }
                
                return certification.EndDate > DateTime.UtcNow && !string.IsNullOrWhiteSpace(party.CapabilityUrl);
            }

            var partiesResponse = await _partiesQueryService.GetAsync(
                new PartiesRequestArgs(accessToken, certifiedOnly: true, activeOnly: true),
                token);

            if (partiesResponse.Count <= 10)
            {
                return partiesResponse.Parties.Where(IsValidIdp).ToArray();
            }

            var parties = await RetrieveAllPartiesAsync(accessToken, partiesResponse, token);
            return parties.Where(IsValidIdp).ToArray();
        }

        private async Task<List<Party>> RetrieveAllPartiesAsync(
            string accessToken,
            PartiesResponse partiesResponse,
            CancellationToken token)
        {
            static int CalculateTotalPages(PartiesResponse partiesResponse)
            {
                var pagesCount = partiesResponse.Count / 10;
                if (partiesResponse.Count % 10 != 0)
                {
                    pagesCount++;
                }

                return pagesCount;
            }

            var parties = new List<Party>(partiesResponse.Parties);
            var calls = new List<Task<PartiesResponse>>();

            var pages = CalculateTotalPages(partiesResponse);
            for (var i = 1; i < pages; i++)
            {
                var call = _partiesQueryService.GetAsync(
                    new PartiesRequestArgs(accessToken, certifiedOnly: true, activeOnly: true, page: i),
                    token);
                calls.Add(call);
            }

            await Task.WhenAll(calls);
            foreach (var call in calls)
            {
                parties.AddRange(call.Result.Parties);
            }

            return parties;
        }

        private async Task<List<IdentityProvider>> RetrieveAuthorizeUrlsAsync(
            ICollection<Party> parties,
            string accessToken,
            CancellationToken token)
        {
            var calls = parties
                .Select(party => new CapabilitiesRequestArgs(party.CapabilityUrl, party.PartyId, accessToken))
                .Select(args => GetCapabilitiesAsync(args, token))
                .ToArray();

            await Task.WhenAll(calls);

            var results = new List<IdentityProvider>();
            foreach (var call in calls)
            {
                var result = call.Result;
                if (result == null)
                {
                    continue;
                }

                var partyName = parties.FirstOrDefault(x => x.PartyId == result.PartyId)?.PartyName;
                var uri = result.SupportedVersions?.FirstOrDefault()
                    ?.SupportedFeatures?.FirstOrDefault()
                    ?.Public?.FirstOrDefault(x => x.Url.AbsoluteUri.Contains("authorize"))?.Url;

                if (partyName != null && uri != null)
                {
                    uri = new Uri(FormatIdpUri(uri.AbsoluteUri));
                    results.Add(new IdentityProvider(partyName, uri));
                }
            }

            return results;
        }
        
        private async Task<CapabilitiesResponse> GetCapabilitiesAsync(
            CapabilitiesRequestArgs args,
            CancellationToken token)
        {
            try
            {
                return await _capabilitiesQueryService.GetAsync(args, token);
            }
            catch (UnsuccessfulResponseException e)
            {
                _logger.LogError(e, "Couldn't retrieve capabilities for party {}");

                return null;
            }
        }
    }
}
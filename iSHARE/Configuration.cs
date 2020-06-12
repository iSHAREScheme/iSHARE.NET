using iSHARE.AccessToken;
using iSHARE.Capabilities;
using iSHARE.IdentityProviders;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Parties;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.SchemeOwner;
using iSHARE.TokenValidator.SchemeOwner.RootCertificate;
using iSHARE.TokenValidator.SchemeOwner.TestCaStrategy;
using iSHARE.TrustedList;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace iSHARE
{
    public static class Configuration
    {
        /// <summary>
        /// Registers default iSHARE settings implementation.
        /// In order for it to work, make sure that appsettings.json file contains <see cref="IShareSettings"/> required properties at root level.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="configuration">Configuration which contains app settings.</param>
        /// <returns>The same contract. Used to make methods chainable.</returns>
        public static IServiceCollection RegisterDefaultSettings(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IShareSettings, DefaultSettings>(provider => new DefaultSettings(configuration));

            return services;
        }

        /// <summary>
        /// Adds essential iSHARE services to Dependency Injection container.
        /// Make sure to register iSHARE settings, otherwise these services won't work.
        /// If you'd like to use default settings, refer to <see cref="RegisterDefaultSettings"/> method.
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// <param name="isProduction">
        /// Specifies if plugins should be registered for production use. It has affect for root certification store.
        /// </param>
        /// <returns>The same contract. Used to make methods chainable.</returns>
        public static IServiceCollection AddIShareServices(this IServiceCollection services, bool isProduction = true)
        {
            services.AddCertificateServices(isProduction);
            services.AddInternalServices();
            services.AddAccessTokenServices();
            services.AddQueryServices();
            services.AddJwtTokenValidatorServices();

            return services;
        }

        private static void AddInternalServices(this IServiceCollection services)
        {
            services.AddTransient<ITokenResponseClient, TokenResponseClient>();
        }

        private static void AddAccessTokenServices(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IAccessTokenStorage, DistributedCacheAccessTokenStorage>();
            services.AddHttpClient<IAccessTokenClient, AccessTokenClient>();
            services.AddScoped<IAccessTokenAccessor, AccessTokenAccessor>();
        }

        private static void AddJwtTokenValidatorServices(this IServiceCollection services)
        {
            services.AddTransient<ISchemeOwnerJwtTokenResponseValidator, SchemeOwnerJwtTokenResponseValidator>();
            services.AddTransient<IDecodedJwtValidator, DecodedJwtValidator>();
            services.AddTransient<IJwtCertificateValidator, JwtCertificateValidator>();
            services.AddTransient<IJwtTokenParser, JwtTokenParser>();
            services.AddTransient<IJwtTokenResponseValidator, JwtTokenResponseValidator>();
        }

        private static void AddQueryServices(this IServiceCollection services)
        {
            services.AddTransient<ITrustedListQueryService, TrustedListQueryService>();
            services.AddTransient<IPartiesQueryService, PartiesQueryService>();
            services.AddTransient<ICapabilitiesQueryService, CapabilitiesQueryService>();
            services.AddTransient<IIdentityProvidersQueryService, IdentityProvidersQueryService>();
        }

        private static void AddCertificateServices(this IServiceCollection services, bool isProduction)
        {
            if (isProduction)
            {
                services.AddTransient<ITestCaStrategy, ProductionCaStrategy>();
                services.AddSingleton<IRootCertificateStorage, ProductionEnvironmentRootCertificateStorage>();
            }
            else
            {
                services.AddSingleton<IRootCertificateStorage, TestEnvironmentRootCertificateStorage>();
                services.AddTransient<ITestCaStrategy, TestCaStrategy>();
            }
        }
    }
}

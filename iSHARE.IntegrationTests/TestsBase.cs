using System.IO;
using System.Threading.Tasks;
using iSHARE.AccessToken;
using iSHARE.AccessToken.Args;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace iSHARE.IntegrationTests
{
    public class TestsBase
    {
        static TestsBase()
        {
            Configuration = BuildConfiguration();
            ServiceScope = CreateServiceScope();
        }

        protected static IConfiguration Configuration { get; }

        protected static IServiceScope ServiceScope { get; }

        protected static async Task<string> GetAccessTokenAsync()
        {
            var clientAssertion = JwtUtilities.Create(
                issuer: Constants.AbcParty.ClientId,
                audience: Constants.SchemeOwner.ClientId,
                Constants.AbcParty.PrivateKey,
                Constants.AbcParty.PublicKeyBase64Der);
            var requestArgs = CreateRequestArgs(clientAssertion);

            var accessTokenAccessor = ServiceScope.ServiceProvider.GetRequiredService<IAccessTokenAccessor>();
            return await accessTokenAccessor.GetAsync(requestArgs);
        }

        private static AccessTokenRequestArgs CreateRequestArgs(
            string clientAssertion,
            string requestUri = Constants.SchemeOwner.AccessTokenRequestUri) =>
            new AccessTokenRequestArgs(requestUri, Constants.AbcParty.ClientId, clientAssertion);

        private static IConfigurationRoot BuildConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        private static IServiceScope CreateServiceScope() =>
            new HostBuilder()
                .ConfigureServices((context, services) => RegisterServices(services))
                .UseConsoleLifetime()
                .Build()
                .Services.CreateScope();

        private static void RegisterServices(IServiceCollection services)
        {
            services.RegisterDefaultSettings(Configuration);
            services.AddIShareServices(isProduction: false);
        }
}
}

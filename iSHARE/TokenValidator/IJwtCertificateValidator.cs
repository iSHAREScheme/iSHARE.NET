using System.Threading;
using System.Threading.Tasks;
using iSHARE.TokenValidator.Args;

namespace iSHARE.TokenValidator
{
    internal interface IJwtCertificateValidator
    {
        Task<bool> IsValidAsync(
            CertificateValidationArgs validationArgs,
            string schemeOwnerAccessToken,
            CancellationToken cancellationToken = default);
    }
}

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace iSHARE.TokenValidator.SchemeOwner.TestCaStrategy
{
    /// <summary>
    /// Used to inject Test CA specific logic for iSHARE certificates.
    /// iSHARE has its own certificate authority which does not contain CRL nor is trusted on user machines.
    /// For production use with proper CAs those should be checked, but for test use they could be skipped.
    /// </summary>
    internal interface ITestCaStrategy
    {
        X509RevocationMode GetRevocationMode();

        bool ShouldErrorsBeIgnored(ICollection<X509ChainStatus> statuses);
    }
}
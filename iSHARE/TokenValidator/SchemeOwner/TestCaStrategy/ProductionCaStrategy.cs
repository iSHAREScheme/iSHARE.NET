using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace iSHARE.TokenValidator.SchemeOwner.TestCaStrategy
{
    internal class ProductionCaStrategy : ITestCaStrategy
    {
        public X509RevocationMode GetRevocationMode() => X509RevocationMode.Online;

        public bool ShouldErrorsBeIgnored(ICollection<X509ChainStatus> statuses) => false;
    }
}

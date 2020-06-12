using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace iSHARE.TokenValidator.SchemeOwner.TestCaStrategy
{
    public class TestCaStrategy : ITestCaStrategy
    {
        public X509RevocationMode GetRevocationMode() => X509RevocationMode.NoCheck;

        public bool ShouldErrorsBeIgnored(ICollection<X509ChainStatus> statuses) =>
            statuses.Any() && statuses.All(c => c.Status.HasFlag(X509ChainStatusFlags.UntrustedRoot));
    }
}

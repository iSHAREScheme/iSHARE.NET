using FluentAssertions;
using iSHARE.TokenValidator;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace iSHARE.Tests.TokenValidator
{
    public class SecurityKeysExtractorTests
    {
        [Fact]
        public void Extract_CollectionPassed_ReturnsExtractedKeys()
        {
            const string cert1 =
                "MIIEgTCCAmmgAwIBAgIIS90K+1Q9HOkwDQYJKoZIhvcNAQELBQAwSDEZMBcGA1UEAwwQaVNIQVJFVGVzdENBX1RMUzEN" +
                "MAsGA1UECwwEVGVzdDEPMA0GA1UECgwGaVNIQVJFMQswCQYDVQQGEwJOTDAeFw0xOTAyMTUxMTQ3MTVaFw0yMTAyMTQx" +
                "MTQ3MTVaMEIxFTATBgNVBAMMDFdhcmVob3VzZSAxMzEcMBoGA1UEBRMTRVUuRU9SSS5OTDAwMDAwMDAwMzELMAkGA1UE" +
                "BhMCTkwwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCwaDAzEJ28DKTFdPPu9oJ4Jt64pKxAHsP3FHLyY57U" +
                "ipdB9mo157xf91z5vZMcNMuqUuau8OHTzlykPA9tCEOocE0DYwR7G0E3Qznq9V8UG/hcn6xeqcRv55wE/ld7EQUuJxYh" +
                "ruUUp6w8KyXBvVttdW8E2yqkyCVaBE50DsshYqk7giQCHLmVXRJn8t6CunvtiHtunO0Z3Xe3E5MRfkCjNncj7OMTMCXv" +
                "8uCNSd8N2teubhT3gABzW2Pju2EyNxJezydSIu9y3XkugeCJIGHFY8LKReu9aEzt8aL/My7Z8CJjIVG1cD0fDFt4JZxu" +
                "YwW8D/njl4CmOLDaGepgGGwfAgMBAAGjdTBzMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoAUFjznIOnWlO8f5aLxuPy+" +
                "6t8sN4EwEwYDVR0lBAwwCgYIKwYBBQUHAwEwHQYDVR0OBBYEFGbovV7FLM52Fca+YE2dEqo+KoxqMA4GA1UdDwEB/wQE" +
                "AwIFoDANBgkqhkiG9w0BAQsFAAOCAgEAJtvE2iAapuG8yroVyei4GvtjfM+IOths1Vwj3Z4tNtWjW8U1YqqgwfAI8gPu" +
                "CzI6HGMD0rrvzIS6tg4peTzDEdY6VfC4ltmcS5cphYYqs6X0wifTp4VVbPt+6GxpMe9oMBTcAGRHTaxW2faPsrQmfDnh" +
                "chHFlSajhqxD5gR/FYvXeAGsoP0BBqmRRDbRtydPxBwuYQrU7TJd+irPvhCVoA0COCAY3ihSsyPpn+LCbrlCbd5SFDpH" +
                "9aWtt62ijCs8MdDNMP2l5lbwrQB7v1bsFzY1SxSCD1E8ymXkBQRhCBcg0mk3EsSVjd/GBXnvCtB77eUF80gL1DxW2pOh" +
                "nMNCt30q1gzd/j2tmcKoKoGznPQC/sd5k4som/+HtOYQbSmYbAnKraIUgjRd3a3uS1XBFGZ3/zoIMsn1qNvr5pCOFBVq" +
                "yKTJtKVcG7/f/gqx1tje8esWG8n6QyIOZ0Wma3VpZVqDdaXDr+J/kCaYp/HmFvF8CVCCBRRWO6j4zQxturq/VaeLL8+I" +
                "b9SoQoFWyRpLnFiyu7zo5qLA6X5Q8Scz0/JN4INqnDhPLadk/JwdAtD/4xPYVVTaAXsheBke+/9A/ggKIjJhk9aPEv8M" +
                "JrxCkAufmbU/iTUXauiPhchc8Fa0gNHWDk9mg072zNGEmaRSPJ2Pfnvw3nMxCjzex5Nw0t4=";
            const string cert2 =
                "MIIEiDCCAnCgAwIBAgIIeDIrdnYo2ngwDQYJKoZIhvcNAQELBQAwSDEZMBcGA1UEAwwQaVNIQVJFVGVzdENBX1RMUzEN" +
                "MAsGA1UECwwEVGVzdDEPMA0GA1UECgwGaVNIQVJFMQswCQYDVQQGEwJOTDAeFw0xOTAyMTUxMTQ2NThaFw0yMTAyMTQx" +
                "MTQ2NThaMEkxHDAaBgNVBAMME2lTSEFSRSBTY2hlbWUgT3duZXIxHDAaBgNVBAUTE0VVLkVPUkkuTkwwMDAwMDAwMDAx" +
                "CzAJBgNVBAYTAk5MMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0RoohePL02Nv4BZTJ3kp7nKshsmjrcr8" +
                "MBaAhQpZZAsgTAlmQiCTPm3c8qYPqN+TuggFWCNn+9W54C5UGqsIwtXTk3YexAwZ4ojRRto8l1HPDVAS6WvW74ACNZlE" +
                "gGwjrCGy2+M5QP7O7wB0T6oFBofRwHZGzgbtSbSQhuqwUxf0GZI8xAl2/GTH25Vfp9T71JEpoZ9ksP3CIi5BHklbT5GK" +
                "xEOFfdMMupX7mWnNQbLxuQpAtGCuorGfPFE7F5evE1zopwceA5FsE1LaBRqt+EOpPImSajR02bchK9jS6nYEWs/FZGLt" +
                "Jall5L3SnZM6OhWxM+lKGzFKw5TIXN9DMwIDAQABo3UwczAMBgNVHRMBAf8EAjAAMB8GA1UdIwQYMBaAFBY85yDp1pTv" +
                "H+Wi8bj8vurfLDeBMBMGA1UdJQQMMAoGCCsGAQUFBwMBMB0GA1UdDgQWBBRfwijT75dIKPlFC/CtRDqUKX9TNjAOBgNV" +
                "HQ8BAf8EBAMCBaAwDQYJKoZIhvcNAQELBQADggIBAK4PWTq/dqtVm+4WCd1KQJ4tj+n4ccIAYLDMqYSBJsN6Q2ctMRC/" +
                "++yM/owPHBriTzwW/joApNPeZh1ITTgSza38m3hxoDq1ux6HVGyK5QCQojFdlec7tOHlmcbuyV4CEyMZbG+yLmVDI3q5" +
                "3VAPgWvKIi2RUpsPNw2lo6H66vHNp5fipB/tE4CDla/TaN51lN3lXOw4ltbZbzbd6MxIlECQfJH5ePripakJhniVkZvQ" +
                "VkaKAepSX0aDYLOpQQnetDWZoVJKAsGEL3HLihqYsDz7/BT3DwT0KC5+98gjGZwvLwetJ96KXTAE502f3jOyP7DD6uK+" +
                "JKgvQZyvI5/Eup0TtNlRfJy8ad8px+38oIxGAlbsKoWmz0oaMGC+lVGNP2M48M7VktBTpymqxVwtTkvMPjZWHKlXt2W3" +
                "3mNKGjJS95I5vqOKPWSSsWdJRYJClmErmidNW31lWAVPr1iSS4JXDvYPLCP44a4ed0GaWZRv/b+D2+QUgOb8SzmjPvf/" +
                "2GNtqWRdyYtXZ9vx034i+XF/ye8siN+X+wFHtRumtswb+scQf4fU6MjKBKUsPDARAGjHj5xHBDjq84njGuQcl0hbS3+Z" +
                "SV4NqkJIUs2LUt9Ev1X7aB7/95EXAcgNXd3KPVm3p8ND75AqM0FGQEaQyOwqKcqTClJCufGsUY4s7rWR";
            var certificates = new[] { cert1, cert2 };

            var result = SecurityKeysExtractor.Extract(certificates);

            result.Should().HaveCount(2);
            result.Should().AllBeOfType<X509SecurityKey>();
        }
    }
}

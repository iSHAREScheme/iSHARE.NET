namespace iSHARE.IntegrationTests
{
    internal class Constants
    {
        public class SchemeOwner
        {
            public const string AccessTokenRequestUri = "https://scheme.isharetest.net/connect/token";

            public const string CapabilitiesRequestUri = "https://scheme.isharetest.net/capabilities";

            public const string ClientId = "EU.EORI.NL000000000";

            public const string PublicKeyBase64Der =
                "MIIEiDCCAnCgAwIBAgIIeDIrdnYo2ngwDQYJKoZIhvcNAQELBQAwSDEZMBcGA1UEA" +
                "wwQaVNIQVJFVGVzdENBX1RMUzENMAsGA1UECwwEVGVzdDEPMA0GA1UECgwGaVNIQV" +
                "JFMQswCQYDVQQGEwJOTDAeFw0xOTAyMTUxMTQ2NThaFw0yMTAyMTQxMTQ2NThaMEk" +
                "xHDAaBgNVBAMME2lTSEFSRSBTY2hlbWUgT3duZXIxHDAaBgNVBAUTE0VVLkVPUkku" +
                "TkwwMDAwMDAwMDAxCzAJBgNVBAYTAk5MMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AM" +
                "IIBCgKCAQEA0RoohePL02Nv4BZTJ3kp7nKshsmjrcr8MBaAhQpZZAsgTAlmQiCTPm" +
                "3c8qYPqN+TuggFWCNn+9W54C5UGqsIwtXTk3YexAwZ4ojRRto8l1HPDVAS6WvW74A" +
                "CNZlEgGwjrCGy2+M5QP7O7wB0T6oFBofRwHZGzgbtSbSQhuqwUxf0GZI8xAl2/GTH" +
                "25Vfp9T71JEpoZ9ksP3CIi5BHklbT5GKxEOFfdMMupX7mWnNQbLxuQpAtGCuorGfP" +
                "FE7F5evE1zopwceA5FsE1LaBRqt+EOpPImSajR02bchK9jS6nYEWs/FZGLtJall5L" +
                "3SnZM6OhWxM+lKGzFKw5TIXN9DMwIDAQABo3UwczAMBgNVHRMBAf8EAjAAMB8GA1U" +
                "dIwQYMBaAFBY85yDp1pTvH+Wi8bj8vurfLDeBMBMGA1UdJQQMMAoGCCsGAQUFBwMB" +
                "MB0GA1UdDgQWBBRfwijT75dIKPlFC/CtRDqUKX9TNjAOBgNVHQ8BAf8EBAMCBaAwD" +
                "QYJKoZIhvcNAQELBQADggIBAK4PWTq/dqtVm+4WCd1KQJ4tj+n4ccIAYLDMqYSBJs" +
                "N6Q2ctMRC/++yM/owPHBriTzwW/joApNPeZh1ITTgSza38m3hxoDq1ux6HVGyK5QC" +
                "QojFdlec7tOHlmcbuyV4CEyMZbG+yLmVDI3q53VAPgWvKIi2RUpsPNw2lo6H66vHN" +
                "p5fipB/tE4CDla/TaN51lN3lXOw4ltbZbzbd6MxIlECQfJH5ePripakJhniVkZvQV" +
                "kaKAepSX0aDYLOpQQnetDWZoVJKAsGEL3HLihqYsDz7/BT3DwT0KC5+98gjGZwvLw" +
                "etJ96KXTAE502f3jOyP7DD6uK+JKgvQZyvI5/Eup0TtNlRfJy8ad8px+38oIxGAlb" +
                "sKoWmz0oaMGC+lVGNP2M48M7VktBTpymqxVwtTkvMPjZWHKlXt2W33mNKGjJS95I5" +
                "vqOKPWSSsWdJRYJClmErmidNW31lWAVPr1iSS4JXDvYPLCP44a4ed0GaWZRv/b+D2" +
                "+QUgOb8SzmjPvf/2GNtqWRdyYtXZ9vx034i+XF/ye8siN+X+wFHtRumtswb+scQf4" +
                "fU6MjKBKUsPDARAGjHj5xHBDjq84njGuQcl0hbS3+ZSV4NqkJIUs2LUt9Ev1X7aB7" +
                "/95EXAcgNXd3KPVm3p8ND75AqM0FGQEaQyOwqKcqTClJCufGsUY4s7rWR";
        }

        public class AuthorizationRegistry
        {
            public const string CapabilitiesRequestUri = "https://ar.isharetest.net/capabilities";

            public const string ClientId = "EU.EORI.NL000000004";
        }

        public class AbcParty
        {
            public const string ClientId = "EU.EORI.NL000000001";

            public const string PrivateKey =
                @"
-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAtDuIVIQ/laDilSGUjTDU383NDtpGI/mc6UBAAWRrT7ctJCjy
XqdPqOcbLaLtSvlMbx38RKR8Rxk3bambnNvC4VOAngoKHgzA0/gh0tzaLIhEkDMI
f2WXk3Pf3igqOevRgp/tl7U4hkqQUsDElqjR4KQGWCnR60/5BDCMkyqGrroikPHF
Ke6MO5mc//Svqld7PS9N9FvCGsCyQSSv9mqQLeY4dY+P5he39KvM4c4awvL9NTy5
WXbk844wa3u5s/sov75WM/3mqdHmE44cSGrclfRRcJZouBQtQjXDLS4fJ7IiaHuX
AZtdyym0BbtxmB++qHi2b1qmz26ddZsZR2vynQIDAQABAoIBAC2saIACg5qHiuP/
/oVJOqj8nlqFNgEn4Hu7rHyt1gP9h5MCTWlvPOCwsFKAV2IIizYdgzBh/pyF52YC
9Uxq5Jf1cUKbiLTUxojw+KFXFnLylW11gVHHjQHw+2pxA0JqYE2Z+bvYUNq3Sput
/iOv5H/pETKGZowbWB+XrggdXsQoOntcHZpPMlSjWpJgkEyRgqbHZHEG+YpUvXq7
iQVbRNzsjBX/ZmQRuH02sQuk04hIksIvMp0BBG1o237V3psGqJuYV7ggCNzwxFAc
J58bdPavdN7itTJ+XytE+RLu3pOw13eoww6reFAocaGScIbwTPPTnhVoN9n1oyCI
8BivUAECgYEA2ASI7nI/M3flxipxb6l8QHUHlz0/agnHGAEzlP1fIlaky+sM2dmN
mZjYJXAfeQI8Jby7Zp8Qe6HG/0psn9kgfDO3GEI982ZAbPZ4su6AAWftFGU24UWv
JtM0danHp2sUyOlqGLgPTMC4E0bdAUl9NsKuzVogeINdL2/ufY8mWaECgYEA1Zdn
9fC0k+VnJmgVtLPhaEfA8YeVB2YWlZJsXI/qOS7JbIB36kfaFENsV/5qkz8MQF5V
Gk8Pk4Rg81qAluGtu09X1pmVg9WrmKsiXRfGC7uGUgiLHoy6lFfCGVpWaeLZ2R5i
9kfX657m/s2alWdI8eJTvwRcAttl0k3/OA6Zz30CgYEAnB7i7BpLzSHZQdhI9Z+z
ek5adxa/7x4z8whjREm+aO3f2rT9K7FM4naRuLUJOgbAe7Gkay2I1yF71ePvD7P4
A2vI51Jwvann5BhZ8U/n2ZxHvxbLjBI13USFxIg5EKmWcwInOMhF3n3mS6BKd/Sp
91rKxQVfNFoUjgVCgyJjYCECgYEAilKVJvcNWsIzvPGnLPFZRrlBQKm3X6VDdvXu
aztgSkv8ceH4xqZlmr3XSl0fE+C+xGjS4EjXI4eVacPwhsX8RtOpZPbtlwUnLaWW
iYc94Jc+851Iyy4EHtF+iACy+bvJrQw6tpfsQ1ES82yfcaYD3XHfHlNqawc+t6V/
ZQQ/o9kCgYAl9LGQxyXfaas7eV5YRmVOB8mZ0NYfgpsRe69K4gfftyoLTlsluN1+
8o//zN6D9jpBK8oa3Egc7TONBrdF5zv+3XMcu1iD3PaGQdhaMX14IM6CyVNfliMq
f+8namfLH2H5UWmEzhLwNKIBJIbz+OOIXaf6Tix0vh3nCmClt7v7lA==
-----END RSA PRIVATE KEY-----";

            public const string PublicKeyBase64Der =
                "MIIEgTCCAmmgAwIBAgIIN9ViCDi3BwswDQYJKoZIhvcNAQELBQAwSDEZMBcGA1UEA" +
                "wwQaVNIQVJFVGVzdENBX1RMUzENMAsGA1UECwwEVGVzdDEPMA0GA1UECgwGaVNIQV" +
                "JFMQswCQYDVQQGEwJOTDAeFw0xOTAyMTUxMTQ2MTVaFw0yMTAyMTQxMTQ2MTVaMEI" +
                "xFTATBgNVBAMMDEFCQyBUcnVja2luZzEcMBoGA1UEBRMTRVUuRU9SSS5OTDAwMDAw" +
                "MDAwMTELMAkGA1UEBhMCTkwwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBA" +
                "QC0O4hUhD+VoOKVIZSNMNTfzc0O2kYj+ZzpQEABZGtPty0kKPJep0+o5xstou1K+U" +
                "xvHfxEpHxHGTdtqZuc28LhU4CeCgoeDMDT+CHS3NosiESQMwh/ZZeTc9/eKCo569G" +
                "Cn+2XtTiGSpBSwMSWqNHgpAZYKdHrT/kEMIyTKoauuiKQ8cUp7ow7mZz/9K+qV3s9" +
                "L030W8IawLJBJK/2apAt5jh1j4/mF7f0q8zhzhrC8v01PLlZduTzjjBre7mz+yi/v" +
                "lYz/eap0eYTjhxIatyV9FFwlmi4FC1CNcMtLh8nsiJoe5cBm13LKbQFu3GYH76oeL" +
                "ZvWqbPbp11mxlHa/KdAgMBAAGjdTBzMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoA" +
                "UFjznIOnWlO8f5aLxuPy+6t8sN4EwEwYDVR0lBAwwCgYIKwYBBQUHAwEwHQYDVR0O" +
                "BBYEFAPH+8UrYiVLXaKPRGflkA+cusQ7MA4GA1UdDwEB/wQEAwIFoDANBgkqhkiG9" +
                "w0BAQsFAAOCAgEAYl5tWH0YtYS9JyqylZJWomA55ShKllaBRcvro6CHxllJWHo0qC" +
                "9ZTMcvCywMv14VJyQd6eFZqcVtOluGrRZBklH9Aankovp2JLaqcD79t1CyuXZnIzT" +
                "Fl/BkMsE6wlAJXY/sarnoxeiejP4E/Ef/0euIFvBaICCF+Kd2WJYbbn0Wy0dH484Q" +
                "JbHyMtVfr42oIpUNVuLSu84yKYAem9JBuYTp3Y0K2hiEAW/bOKDvvHetVf5fu66yf" +
                "ekDX53j3NKiFJCXS2rKIZoDuMFuxpSyVkS2kbWk1+5Joy7qOSNANRFPlpHcgzLQZp" +
                "8HrgvhsmhIt1VTVYkyxcd8qXAlhwqVgOq5NgLxkqul9hNMGiM7sq+H73Q/Fi8if7g" +
                "P8IVASzQGwuHg2Z87ib6A2von0fRJZq3fIubHoxI63ATwfcRK86y172xbFE3VU0du" +
                "N1tI5Z0TCg0GAJYtJpbnvexIt5lk5FIk4ThvR0L8mNLy1DUa11N+EMxkqbfqlTurB" +
                "8Zg3CY/QaSKmXY5CMUwWeEBXRHhyfkZQ5jPPUHrFaoyORX8p1ErYDktB1K9o4jmuE" +
                "VpB33cvgYBEiAyV5z4426FuVCMbHaFDV7iKW9eBlXoxeZo4XX8+jXySL5GW8XwNRT" +
                "K5c4vW02Q3VJyYVe5umestsKQ+LR8iAzoUSreK18+JkAjAJU=";
        }
    }
}

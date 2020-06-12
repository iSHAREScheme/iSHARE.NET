using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace iSHARE.TokenValidator
{
    public static class CertificateUtilities
    {
        /// <summary>
        /// Converts string into X509Certificate2 object.
        /// </summary>
        /// <param name="certificate">String representation of certificate. Should start with '-----BEGIN CERTIFICATE-----'.</param>
        /// <returns>Certificate object</returns>
        /// <exception cref="ArgumentNullException">Throws if input is null.</exception>
        /// <exception cref="EncoderFallbackException">Throws if fallback occurs, a.k.a. is unable to handle invalid character.</exception>
        /// <exception cref="CryptographicException">Throws if can't create certificate due to corrupted input value.</exception>
        public static X509Certificate2 FromPemFormat(string certificate) =>
            new X509Certificate2(Encoding.ASCII.GetBytes(certificate));

        /// <summary>
        /// Converts string into X509Certificate2 object.
        /// </summary>
        /// <param name="certificate">String representation of certificate. Should be BASE64 encoded string.</param>
        /// <returns>Certificate object</returns>
        /// <exception cref="ArgumentNullException">Throws if input is null.</exception>
        /// <exception cref="FormatException">Throws if input is not a valid base64 string.</exception>
        /// <exception cref="CryptographicException">Throws if can't create certificate due to corrupted input value.</exception>
        public static X509Certificate2 FromBase64Der(string certificate) =>
            new X509Certificate2(Convert.FromBase64String(certificate));

        /// <summary>
        /// Gets certificate's SHA256 (a.k.a. iSHARE fingerprint).
        /// </summary>
        /// <param name="cert">Certificate.</param>
        /// <returns>Fingerprint.</returns>
        /// <exception cref="ArgumentNullException">Throws if object is null.</exception>
        public static string GetSha256(this X509Certificate2 cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException(nameof(cert));
            }

            using var hasher = new SHA256Managed();
            var hashBytes = hasher.ComputeHash(cert.RawData);

            return BitConverter.ToString(hashBytes).Replace("-", "", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}

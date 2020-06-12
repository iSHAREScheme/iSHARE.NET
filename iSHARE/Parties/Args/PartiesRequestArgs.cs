using System;

namespace iSHARE.Parties.Args
{
    public class PartiesRequestArgs
    {
        /// <summary>
        /// Constructor for object which is used to encapsulate <see cref="IPartiesQueryService"/> parameters.
        /// </summary>
        /// <param name="accessToken">Access token which is going to be used in the header for authorization.</param>
        /// <param name="name">Optional parameter used to search by party’s name. Can contain a single * as wildcard.</param>
        /// <param name="eori">Optional parameter used to search by party’s EORI. Can contain a single * as wildcard.</param>
        /// <param name="certifiedOnly">Optional parameter used to search all certified parties.</param>
        /// <param name="activeOnly">Optional parameter used to search all active parties.</param>
        /// <param name="certificateSubjectName">
        /// subjectName as encoded in the X.509 certificate which corresponds with the party that is being requested from the Scheme Owner.
        /// Used by the Scheme Owner to match the certificate identifier.
        /// Subject name attributes may be in any order, but all of them must be included and separated by comma.
        /// If at least one subject attribute is missing - information won’t be returned.
        /// Only returns info if combined with the valid <see cref="Eori"/> associated to it.
        /// </param>
        /// <param name="page">Optional parameter used for navigation in case the result contains more than 10 parties.</param>
        /// <param name="dateTime">Date time for which the information is requested. If provided the result becomes final and therefore cacheable.</param>
        /// <exception cref="ArgumentNullException">Throws if <see cref="accessToken"/> is null or whitespace or all parameters are null.</exception>
        public PartiesRequestArgs(
            string accessToken,
            string name = null,
            string eori = null,
            bool? certifiedOnly = null,
            bool? activeOnly = null,
            string certificateSubjectName = null,
            int? page = null,
            DateTime? dateTime = null)
        {
            ValidateArguments(
                accessToken,
                name,
                eori,
                certifiedOnly,
                activeOnly,
                certificateSubjectName,
                page,
                dateTime);

            AccessToken = accessToken;
            Name = name;
            Eori = eori;
            CertifiedOnly = certifiedOnly;
            ActiveOnly = activeOnly;
            CertificateSubjectName = certificateSubjectName;
            Page = page;
            DateTime = dateTime;
        }

        /// <summary>
        /// Access token which is going to be used in the header for authorization.
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// Optional parameter used to search by party’s name. Can contain a single * as wildcard.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Optional parameter used to search by party’s EORI. Can contain a single * as wildcard.
        /// </summary>
        public string Eori { get; }

        /// <summary>
        /// Optional parameter used to search all certified parties.
        /// </summary>
        public bool? CertifiedOnly { get; }

        /// <summary>
        /// Optional parameter used to search all active parties.
        /// </summary>
        public bool? ActiveOnly { get; }

        /// <summary>
        /// subjectName as encoded in the X.509 certificate which corresponds with the party that is being requested from the Scheme Owner.
        /// Used by the Scheme Owner to match the certificate identifier.
        /// Subject name attributes may be in any order, but all of them must be included and separated by comma.
        /// If at least one subject attribute is missing - information won’t be returned.
        /// Only returns info if combined with the valid <see cref="Eori"/> associated to it.
        /// </summary>
        public string CertificateSubjectName { get; }

        /// <summary>
        /// Optional parameter used for navigation in case the result contains more than 10 parties.
        /// </summary>
        public int? Page { get; }

        /// <summary>
        /// Date time for which the information is requested. If provided the result becomes final and therefore cacheable.
        /// </summary>
        public DateTime? DateTime { get; }

        private static void ValidateArguments(
            string accessToken,
            string name = null,
            string eori = null,
            bool? certifiedOnly = null,
            bool? activeOnly = null,
            string certificateSubjectName = null,
            int? page = null,
            DateTime? dateTime = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (name == null
                && eori == null
                && certifiedOnly == null
                && activeOnly == null
                && certificateSubjectName == null
                && page == null)
            {
                throw new ArgumentNullException(
                    "ALL",
                    $"At least one parameter in addition to {nameof(accessToken)} must be provided.");
            }
        }
    }
}

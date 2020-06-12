namespace iSHARE
{
    public interface IShareSettings
    {
        public string SchemeOwnerUrl { get; }

        /// <summary>
        /// Your organization's EORI number. Used to verify if JWT token audience is correct.
        /// </summary>
        public string Eori { get; }
    }
}

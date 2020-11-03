namespace iSHARE
{
    public interface IShareSettings
    {
        string SchemeOwnerUrl { get; }

        /// <summary>
        /// Your organization's EORI number. Used to verify if JWT token audience is correct.
        /// </summary>
        string Eori { get; }
    }
}

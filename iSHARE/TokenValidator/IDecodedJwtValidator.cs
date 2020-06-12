using iSHARE.TokenValidator.Args;

namespace iSHARE.TokenValidator
{
    internal interface IDecodedJwtValidator
    {
        bool IsIShareCompliant(TokenValidationArgs args);
    }
}

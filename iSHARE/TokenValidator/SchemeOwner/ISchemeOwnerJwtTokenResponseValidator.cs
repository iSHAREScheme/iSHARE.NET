using iSHARE.TokenValidator.Models;

namespace iSHARE.TokenValidator.SchemeOwner
{
    internal interface ISchemeOwnerJwtTokenResponseValidator
    {
        bool IsValid(AssertionModel assertionModel);
    }
}

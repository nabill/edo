namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public interface IPropertyOwnerConfirmationUrlGenerator
    {
        string Generate(string referenceCode);
        string ReadReferenceCode(string encryptedReferenceCode);
    }
}
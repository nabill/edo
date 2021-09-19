namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public enum DeactivationReasons
    {
        None = 0,
        InvalidCoordinates = 1,
        InvalidName = 2,
        DuplicateInOneSupplier = 3,
        MatchingWithOther = 4,
        DeactivatedOnSupplier = 5,
        WrongMatching = 6
    }
}
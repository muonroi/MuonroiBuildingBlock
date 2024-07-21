namespace MuonroiBuildingBlock.Contract.Enums
{
    public enum MuonroiLoginResultType : byte
    {
        Success = 1,
        InvalidUserNameOrEmailAddress,
        InvalidPassword,
        UserIsNotActive,
        InvalidTenancyName,
        TenantIsNotActive,
        UserEmailIsNotConfirmed,
        UnknownExternalLogin,
        LockedOut,
        UserPhoneNumberIsNotConfirmed,
        FailedForOtherReason
    }
}
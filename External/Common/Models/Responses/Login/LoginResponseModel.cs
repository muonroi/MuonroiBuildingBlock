namespace Muonroi.BuildingBlock.External.Common.Models.Responses.Login
{
    public class LoginResponseModel
    {
        public virtual string AccessToken { get; set; } = string.Empty;
        public virtual string RefreshToken { get; set; } = string.Empty;
        public virtual string EmailAddress { get; set; } = string.Empty;
        public virtual string Name { get; set; } = string.Empty;

        public virtual string Surname { get; set; } = string.Empty;

        [NotMapped]
        public virtual string FullName => Name + " " + Surname;
        public virtual string PhoneNumber { get; set; } = string.Empty;
        public virtual bool IsPhoneNumberConfirmed { get; set; }
        public virtual bool IsEmailConfirmed { get; set; }
        public virtual bool IsActive { get; set; }
    }
}

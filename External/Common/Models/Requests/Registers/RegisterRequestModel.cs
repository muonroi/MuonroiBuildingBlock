namespace Muonroi.BuildingBlock.External.Common.Models.Requests.Registers
{
    public class RegisterRequestModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public bool IsTwoFactorEnabled { get; set; }

    }
}

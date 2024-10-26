namespace BrightFocus.Core.Models.Requests.Login
{
    public class RefreshTokenRequestModel
    {
        public virtual string AccessToken { get; set; } = string.Empty;
        public virtual string RefreshToken { get; set; } = string.Empty;
    }
}

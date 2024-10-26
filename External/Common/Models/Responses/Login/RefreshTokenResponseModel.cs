namespace Muonroi.BuildingBlock.External.Common.Models.Responses.Login
{
    public class RefreshTokenResponseModel
    {
        public virtual string AccessToken { get; set; } = string.Empty;
        public virtual string RefreshToken { get; set; } = string.Empty;
    }
}

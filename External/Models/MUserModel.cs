namespace Muonroi.BuildingBlock.External.Models
{
    public class MUserModel(string userGuid, string username, string tokenValidity)
    {
        public string UserGuid { get; set; } = userGuid;
        public string Username { get; set; } = username;
        public string TokenValidity { get; set; } = tokenValidity;
    }
}
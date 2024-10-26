namespace Muonroi.BuildingBlock.External.Models
{
    public class MUserModel(string userGuid, string username)
    {
        public string UserGuid { get; set; } = userGuid;
        public string Username { get; set; } = username;
    }
}
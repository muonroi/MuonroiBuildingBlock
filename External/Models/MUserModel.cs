namespace Muonroi.BuildingBlock.External.Models
{
    public class MUserModel(string userGuid, string username, IEnumerable<string> roles)
    {
        public string UserGuid { get; set; } = userGuid;
        public string Username { get; set; } = username;
        public IEnumerable<string> Roles { get; set; } = roles;
    }
}
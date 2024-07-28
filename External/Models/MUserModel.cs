namespace MBuildingBlock.External.Models
{
    public class MUserModel(string userId, string userGuid, string username, IEnumerable<string> roles)
    {
        public string UserId { get; set; } = userId;
        public string UserGuid { get; set; } = userGuid;
        public string Username { get; set; } = username;
        public IEnumerable<string> Roles { get; set; } = roles;
    }
}
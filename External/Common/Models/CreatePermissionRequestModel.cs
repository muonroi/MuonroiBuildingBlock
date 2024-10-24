namespace Muonroi.BuildingBlock.External.Common.Models
{
    public class CreatePermissionRequestModel
    {
        public required string Name { get; set; }
        public bool IsGranted { get; set; } = true;
        public string Discriminator { get; set; } = "";
    }
}

namespace Muonroi.BuildingBlock.External.Common.Models
{
    public class UpdateRoleRequestModel
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public bool IsStatic { get; set; }
        public bool IsDefault { get; set; }
    }
}

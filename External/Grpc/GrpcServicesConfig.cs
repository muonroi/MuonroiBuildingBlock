namespace Muonroi.BuildingBlock.External.Grpc
{
    public class GrpcServicesConfig
    {
        public Dictionary<string, GrpcServiceConfig> Services { get; set; } = [];
    }

    public class GrpcServiceConfig
    {
        public string Uri { get; set; } = string.Empty;
    }
}
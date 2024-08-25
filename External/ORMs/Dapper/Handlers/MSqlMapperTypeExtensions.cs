namespace Muonroi.BuildingBlock.External.ORMs.Dapper.Handlers;

public static class MSqlMapperTypeExtensions
{
    public static void RegisterDapperHandlers()
    {
        SqlMapper.ResetTypeHandlers();
        SqlMapper.AddTypeHandler(new MProtobufTimestampHandler());
        SqlMapper.AddTypeHandler(new MTrimStringHandler());
    }
}
namespace Muonroi.BuildingBlock.External.ORMs.Dapper.Handlers;

public class MProtobufTimestampHandler : SqlMapper.TypeHandler<Timestamp>
{
    public override Timestamp Parse(object value)
    {
        return Timestamp.FromDateTime(DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc));
    }

    public override void SetValue(IDbDataParameter parameter, Timestamp? value)
    {
        parameter.Value = value;
    }
}
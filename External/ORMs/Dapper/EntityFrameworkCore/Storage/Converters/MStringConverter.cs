namespace Muonroi.BuildingBlock.External.ORMs.Dapper.EntityFrameworkCore.Storage.Converters;

public class MStringConverter : ValueConverter<string, string>
{
    public MStringConverter()
        : base(
            v => v,
            v => v.Trim())
    {
    }
}
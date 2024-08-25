namespace Muonroi.BuildingBlock.External.Models
{
    public class MPagedResult<T> : MPagedResultModel where T : class
    {
        public IEnumerable<T> Items { get; set; }

        public MPagedResult()
        {
            Items = [];
        }
    }
}
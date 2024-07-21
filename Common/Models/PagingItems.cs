namespace MuonroiBuildingBlock.Common.Models
{
    public class PagingItems<T>
    {
        public IEnumerable<T> Items { get; set; }

        public PagingInfo? PagingInfo { get; set; }
    }
}
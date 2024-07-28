namespace MBuildingBlock.External.Models
{
    public abstract class MPagedResultModel
    {
        public int CurrentPage { get; set; }

        public int PageCount
        {
            get
            {
                double pageCount = (double)RowCount / PageSize;
                return (int)Math.Ceiling(pageCount);
            }
        }

        public int PageSize { get; set; }
        public int RowCount { get; set; }

        public int FirstRowOnPage => (CurrentPage - 1) * PageSize + 1;

        public int LastRowOnPage => Math.Min(CurrentPage * PageSize, RowCount);

        public string? AdditionalData { get; set; }
    }
}
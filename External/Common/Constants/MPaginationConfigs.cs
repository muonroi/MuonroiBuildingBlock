namespace MBuildingBlock.External.Common.Constants
{
    public class MPaginationConfigs(string sectionName = "PaginationConfigs")
    {
        public string SectionName = sectionName;
        public virtual int DefaultPageIndex { get; set; }
        public virtual int DefaultPageSize { get; set; }
        public virtual int MaxPageSize { get; set; }
    }
}
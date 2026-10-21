namespace JtechnApi.Modules.AssemblyTargetStatistics.Models
{
    public class AssemblyCombineTargetSnapshot
    {
        public long TotalRows { get; set; }
        public bool IsTruncated { get; set; }
        public string[] Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
        public List<Dictionary<string, object>> StatusRows { get; set; }
        public List<Dictionary<string, object>> StatusCounts { get; set; }
        public long StatusTotalRows { get; set; }
        public bool StatusIsTruncated { get; set; }
    }
}

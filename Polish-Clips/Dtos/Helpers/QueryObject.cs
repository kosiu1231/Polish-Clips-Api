namespace Polish_Clips.Dtos.Helpers
{
    public class QueryObject
    {
        public string? Name { get; set; } = null;
        public string? Game { get; set; } = null;
        public string? Broadcaster { get; set; } = null;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}

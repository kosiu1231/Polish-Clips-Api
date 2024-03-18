namespace Polish_Clips.Dtos.Report
{
    public class GetReportDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public GetUserDto? User { get; set; }
        public GetClipDto? Clip { get; set; }
        public bool isReviewed { get; set; } = false;
    }
}

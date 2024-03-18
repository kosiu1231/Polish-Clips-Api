namespace Polish_Clips.Dtos.Report
{
    public class AddReportDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Report has to contain a message")]
        [MaxLength(360, ErrorMessage = "Report has to be 360 characters at maximum")]
        public string Text { get; set; } = string.Empty;
        [Required]
        public int ClipId { get; set; }
    }
}

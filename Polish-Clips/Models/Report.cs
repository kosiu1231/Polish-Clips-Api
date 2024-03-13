namespace Polish_Clips.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public User? User { get; set; }
        public Clip? Clip { get; set; }
        public int ClipId { get; set; }
        public bool isReviewed { get; set; } = false;
    }
}

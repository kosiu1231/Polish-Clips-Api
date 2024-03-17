namespace Polish_Clips.Models
{
    public class TwitchAccessToken
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

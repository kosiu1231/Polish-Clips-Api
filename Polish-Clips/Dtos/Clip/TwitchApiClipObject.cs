namespace Polish_Clips.Dtos.Clip
{
    public class TwitchApiClipObject
    {
        public string id { get; set; } = string.Empty;
        public string embed_url { get; set; } = string.Empty;
        public int broadcaster_id { get; set; }
        public string broadcaster_name { get; set; } = string.Empty;
        public int game_id { get; set; }
        public string title { get; set; } = string.Empty;
        public int? view_count { get; set; }
        public DateTime created_at { get; set; }
        public string thumbnail_url { get; set; } = string.Empty;
        public float duration { get; set; }
    }
}

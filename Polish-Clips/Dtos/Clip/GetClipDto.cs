namespace Polish_Clips.Dtos.Clip
{
    public class GetClipDto
    {
        public int Id { get; set; }
        public string TwitchId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EmbedUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string StreamerName { get; set; } = string.Empty;
        public float Duration { get; set; } = 0;
        public int LikeAmount { get; set; } = 0;
        public int CommentAmount { get; set; } = 0;
        public GetUserDto? User { get; set; }
        public GetGameDto? Game { get; set; }
        public List<GetCommentDto>? Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

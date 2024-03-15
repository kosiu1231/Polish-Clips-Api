namespace Polish_Clips.Dtos.Comment
{
    public class GetCommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int ClipId { get; set; }
        public GetUserDto? User { get; set; }
    }
}

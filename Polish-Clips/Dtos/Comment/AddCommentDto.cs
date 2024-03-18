namespace Polish_Clips.Dtos.Comment
{
    public class AddCommentDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Comment has to contain a message")]
        [MaxLength(360, ErrorMessage = "Comment has to be 360 characters at maximum")]
        public string Text { get; set; } = string.Empty;
        [Required]
        public int ClipId { get; set; }
    }
}

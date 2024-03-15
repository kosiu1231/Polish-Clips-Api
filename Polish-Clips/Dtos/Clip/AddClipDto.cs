namespace Polish_Clips.Dtos.Clip
{
    public class AddClipDto
    {
        [Required]
        public string TwitchId { get; set; } = string.Empty;
        [MaxLength(64, ErrorMessage = "Mod name has to be 64 characters at maximum")]
        public string? Title { get; set; } = string.Empty;
    }
}

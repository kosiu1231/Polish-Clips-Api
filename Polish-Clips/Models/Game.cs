namespace Polish_Clips.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ArtUrl { get; set; } = string.Empty;
        public List<Clip>? Clips { get; set; }
    }
}

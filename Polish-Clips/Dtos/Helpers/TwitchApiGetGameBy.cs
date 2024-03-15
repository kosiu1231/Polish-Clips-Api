namespace Polish_Clips.Dtos.Helpers
{
    public class TwitchApiGetGameBy
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool ByName { get; set; } = false;
    }
}

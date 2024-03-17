namespace Polish_Clips.Dtos.Helpers
{
    public class TwitchApiAccessTokenObject
    {
        public string access_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }
}

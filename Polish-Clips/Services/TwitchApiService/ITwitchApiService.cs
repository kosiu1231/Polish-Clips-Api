namespace Polish_Clips.Services.TwitchApiService
{
    public interface ITwitchApiService
    {
        Task<ServiceResponse<string>> AddGame(TwitchApiGetGameBy gameByObject);
        Task<ServiceResponse<string>> AddClipsByStreamers();
        Task<ServiceResponse<string>> AddBroadcasters();
        Task<ServiceResponse<string>> RefreshTwitchAccessToken();
    }
}

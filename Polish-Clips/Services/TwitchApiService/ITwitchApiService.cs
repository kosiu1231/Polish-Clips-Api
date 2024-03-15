namespace Polish_Clips.Services.TwitchApiService
{
    public interface ITwitchApiService
    {
        Task<ServiceResponse<string>> AddGame(TwitchApiGetGameBy gameByObject);
    }
}

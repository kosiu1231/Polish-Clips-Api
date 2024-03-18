namespace Polish_Clips.Services.GameService
{
    public interface IGameService
    {
        Task<ServiceResponse<List<GetGameDto>>> GetGames(QueryObject query);
    }
}

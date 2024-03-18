namespace Polish_Clips.Controllers
{
    [ApiController]
    [Route("api")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet("games")]
        public async Task<ActionResult<ServiceResponse<List<GetGameDto>>>> GetGames([FromQuery] QueryObject query)
        {
            var response = await _gameService.GetGames(query);
            if (response.Data is null)
                return NotFound(response);
            return Ok(response);
        }
    }
}

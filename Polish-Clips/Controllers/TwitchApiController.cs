namespace Polish_Clips.Controllers
{
    [ApiController]
    [Route("twitchapi")]
    public class TwitchApiController : ControllerBase
    {
        private readonly ITwitchApiService _twitchApiService;
        public TwitchApiController(ITwitchApiService twitchApiService)
        {
            _twitchApiService = twitchApiService;
        }

        [HttpPost("game")]
        public async Task<ActionResult<ServiceResponse<string>>> AddGame(TwitchApiGetGameBy gameByObject)
        {
            var response = await _twitchApiService.AddGame(gameByObject);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}

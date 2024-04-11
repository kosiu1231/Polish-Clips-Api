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

        [Authorize(Roles = "Admin")]
        [HttpPost("game")]//only for testing, remove later
        public async Task<ActionResult<ServiceResponse<string>>> AddGame(TwitchApiGetGameBy gameByObject)
        {
            var response = await _twitchApiService.AddGame(gameByObject);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("autoclipsbystreamers")]//only for testing, remove later
        public async Task<ActionResult<ServiceResponse<string>>> AddClipsByStreamers()
        {
            var response = await _twitchApiService.AddClipsByStreamers();
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("autobroadcasters")]//only for testing, remove later
        public async Task<ActionResult<ServiceResponse<string>>> AddBroadcasters()
        {
            var response = await _twitchApiService.AddBroadcasters();
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("refresh")]//only for testing, remove later
        public ActionResult<string> Refresh()
        {
            return Ok("refreshed");
        }
    }
}

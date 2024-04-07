namespace Polish_Clips.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClipController : ControllerBase
    {
        private readonly IClipService _clipService;
        public ClipController(IClipService clipService)
        {
            _clipService = clipService;
        }

        [Authorize]
        [HttpPost("clip")]
        public async Task<ActionResult<ServiceResponse<GetClipDto>>> AddClip (AddClipDto newClip)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _clipService.AddClip(newClip);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("clip")]
        public async Task<ActionResult<ServiceResponse<string>>> DeleteClip(int id)
        {
            var response = await _clipService.DeleteClip(id);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("clip/{id:int}")]
        public async Task<ActionResult<ServiceResponse<GetClipDto>>> GetClip(int id)
        {
            var response = await _clipService.GetClip(id);
            if (response.Data is null)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet("clips")]
        public async Task<ActionResult<ServiceResponse<List<GetClipDto>>>> GetClips([FromQuery] QueryObject query)
        {
            var response = await _clipService.GetClips(query);
            if (response.Data is null)
                return NotFound(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("clip/{id:int}/like")]
        public async Task<ActionResult<ServiceResponse<GetClipDto>>> LikeClip(int id)
        {
            var response = await _clipService.LikeClip(id);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("clip/{id:int}/dislike")]
        public async Task<ActionResult<ServiceResponse<GetClipDto>>> DislikeClip(int id)
        {
            var response = await _clipService.DislikeClip(id);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }
    }
}

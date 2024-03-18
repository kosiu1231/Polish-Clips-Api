using Microsoft.AspNetCore.Authorization;

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
                return NotFound(response);
            return Ok(response);
        }
    }
}

namespace Polish_Clips.Controllers
{
    [ApiController]
    [Route("api")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPost("comment")]
        public async Task<ActionResult<ServiceResponse<GetClipDto>>> AddComment(AddCommentDto newComment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _commentService.AddComment(newComment);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("comment")]
        public async Task<ActionResult<ServiceResponse<GetClipDto>>> DeleteComment(int id)
        {
            var response = await _commentService.DeleteComment(id);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }
    }
}

namespace Polish_Clips.Controllers
{
    [ApiController]
    [Route("api")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [Authorize]
        [HttpPost("report")]
        public async Task<ActionResult<ServiceResponse<GetReportDto>>> AddReport(AddReportDto newReport)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _reportService.AddReport(newReport);
            if (response.Data is null)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("report/{id:int}/review")]
        public async Task<ActionResult<ServiceResponse<GetReportDto>>> ReviewReport(int id)
        {
            var response = await _reportService.ReviewReport(id);
            if (response.Data is null)
                return NotFound(response);
            return Ok(response);
        }
    }
}

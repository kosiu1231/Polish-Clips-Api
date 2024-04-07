using Polish_Clips.Dtos.Report;

namespace Polish_Clips.Services.ReportService
{
    public interface IReportService
    {
        Task<ServiceResponse<GetReportDto>> AddReport(AddReportDto newReport);
        Task<ServiceResponse<List<GetReportDto>>> GetReports();
        Task<ServiceResponse<List<GetReportDto>>> ReviewReport(int id);
    }
}

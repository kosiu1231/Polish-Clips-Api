using Polish_Clips.Dtos.Report;

namespace Polish_Clips.Services.ReportService
{
    public interface IReportService
    {
        Task<ServiceResponse<GetReportDto>> AddReport(AddReportDto newReport);
        Task<ServiceResponse<GetReportDto>> ReviewReport(int id);
    }
}

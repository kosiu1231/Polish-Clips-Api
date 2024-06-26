﻿using Polish_Clips.Dtos.Report;
using Polish_Clips.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Polish_Clips.Services.ReportService
{
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor, ILogger<ReportService> logger)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string GetUserRole() => _httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role)!;

        public async Task<ServiceResponse<GetReportDto>> AddReport(AddReportDto newReport)
        {
            var response = new ServiceResponse<GetReportDto>();

            try
            {
                var report = _mapper.Map<Report>(newReport);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
                var clip = await _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .FirstOrDefaultAsync(c => c.Id == report.ClipId);

                report.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
                report.Clip = clip;

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono klipu.";
                    return response;
                }

                if (await _context.Reports.AnyAsync(r => r.User!.Id == user!.Id && r.Clip!.Id == clip.Id))
                {
                    response.Success = false;
                    response.Message = "Klip został już zgłoszony przez użytkownika.";
                    return response;
                }

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
                response.Data = _mapper.Map<GetReportDto>(report);
                _logger.LogInformation($"Clip {clip.Id} reported by {user!.Username}: {newReport.Text}");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetReportDto>>> GetReports()
        {
            var response = new ServiceResponse<List<GetReportDto>>();

            try
            {
                var reports = _context.Reports
                    .Include(u => u.User)
                    .Include(g => g.Clip).ThenInclude(u => u!.User)
                    .Include(g => g.Clip).ThenInclude(g => g!.Game)
                    .Include(g => g.Clip).ThenInclude(c => c!.Comments)
                    .AsQueryable();

                if (reports.Count() == 0)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono zgłoszeń.";
                    return response;
                }

                response.Data = await reports.Select(r => _mapper.Map<GetReportDto>(r)).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetReportDto>>> ReviewReport(int id)
        {
            var response = new ServiceResponse<List<GetReportDto>>();

            try
            {
                if (GetUserRole() != "Admin")
                {
                    response.Success = false;
                    response.Message = "Nieprawidłowa rola.";
                    return response;
                }

                var report = await _context.Reports
                    .Include(u => u.User)
                    .Include(c => c.Clip)
                    .Include(c => c.Clip!.User)
                    .Include(c => c.Clip!.Game)
                    .FirstOrDefaultAsync(
                    r => r.Id == id);

                if (report is null)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono zgłoszenia.";
                    return response;
                }

                report.isReviewed = true;
                await _context.SaveChangesAsync();

                var reports = _context.Reports
                    .Include(u => u.User)
                    .Include(g => g.Clip).ThenInclude(u => u!.User)
                    .Include(g => g.Clip).ThenInclude(g => g!.Game)
                    .Include(g => g.Clip).ThenInclude(c => c!.Comments)
                    .AsQueryable();

                response.Data = await reports.Select(r => _mapper.Map<GetReportDto>(r)).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}

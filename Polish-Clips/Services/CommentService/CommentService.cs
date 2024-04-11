
namespace Polish_Clips.Services.CommentService
{
    public class CommentService : ICommentService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string GetUserRole() => _httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role)!;

        public async Task<ServiceResponse<GetClipDto>> AddComment(AddCommentDto newComment)
        {
            var response = new ServiceResponse<GetClipDto>();

            try
            {
                var comment = _mapper.Map<Comment>(newComment);
                var clip = await _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .Include(c => c.Comments)
                    .FirstOrDefaultAsync(c => c.Id == comment.ClipId);

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono klipu.";
                    return response;
                }

                comment.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
                comment.Clip = clip;
                comment.CreatedAt = DateTime.Now;
                
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                response.Data = _mapper.Map<GetClipDto>(clip);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<GetClipDto>> DeleteComment(int id)
        {
            var response = new ServiceResponse<GetClipDto>();

            try
            {
                if(GetUserRole() != "Admin")
                {
                    response.Success = false;
                    response.Message = "Nieprawidłowa rola.";
                    return response;
                }    

                var comment = await _context.Comments.FirstOrDefaultAsync(
                    c => c.Id == id);

                if (comment is null)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono komentarza.";
                    return response;
                }

                var clip = await _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .Include(c => c.Comments)
                    .FirstOrDefaultAsync(m => m.Comments!.Any(c => c.Id == id));

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono klipu.";
                    return response;
                }

                clip!.Comments!.Remove(comment);
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<GetClipDto>(clip);
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

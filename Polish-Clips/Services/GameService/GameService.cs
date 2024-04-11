
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Polish_Clips.Services.GameService
{
    public class GameService : IGameService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public GameService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetGameDto>>> GetGames(QueryObject query)
        {
            var response = new ServiceResponse<List<GetGameDto>>();
            try
            {
                var games = _context.Games.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.Name))
                    games = games.Where(g => g.Name.Contains(query.Name));

                if (games.Count() == 0)
                {
                    response.Success = false;
                    response.Message = "Nie znaleziono gier.";
                    return response;
                }

                int skipNumber = (query.PageNumber - 1) * query.PageSize;

                games = games.Skip(skipNumber).Take(query.PageSize);

                response.Data = await games.OrderByDescending(g => g.Clips!.Count)
                    .Select(g => _mapper.Map<GetGameDto>(g)).ToListAsync();
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

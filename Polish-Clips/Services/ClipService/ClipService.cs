
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Polish_Clips.Services.ClipService
{
    public class ClipService : IClipService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ITwitchApiService _twitchApiService;

        public ClipService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ITwitchApiService twitchApiService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _configuration = configuration;
            _twitchApiService = twitchApiService;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string GetUserRole() => _httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role)!;

        public async Task<ServiceResponse<GetClipDto>> AddClip(AddClipDto newClip)
        {
            var response = new ServiceResponse<GetClipDto>();

            try
            {
                var clip = _mapper.Map<Clip>(newClip);
                clip.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

                List<TwitchApiClipObject> clips = new List<TwitchApiClipObject>();

                if (await _context.Clips.AnyAsync(c => c.TwitchId == clip.TwitchId))
                {
                    response.Success = false;
                    response.Message = "Clip already is in database";
                    return response;
                }

                clips = await GetClipFromApi(clip.TwitchId);

                if(clips.Count() < 1)
                {
                    response.Success = false;
                    response.Message = "Clip not found";
                    return response;
                }

                clip.Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == clips[0].game_id);

                if (clip.Game is null)
                {
                    TwitchApiGetGameBy gameByObject = new TwitchApiGetGameBy
                    {
                        Id = clips[0].game_id,
                        ByName = false
                    };

                    await _twitchApiService.AddGame(gameByObject);
                    clip.Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == clips[0].game_id);
                }

                clip.EmbedUrl = clips[0].embed_url;
                clip.StreamerName = clips[0].broadcaster_name;
                clip.CreatedAt = clips[0].created_at;
                clip.ThumbnailUrl = clips[0].thumbnail_url;
                clip.Duration = clips[0].duration;
                if (String.IsNullOrWhiteSpace(clip.Title))
                    clip.Title = clips[0].title;

                _context.Clips.Add(clip);
                clip.Game!.Clips!.Add(clip);
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

        public async Task<List<TwitchApiClipObject>> GetClipFromApi(string clipId)
        {
            string baseUrl = "https://api.twitch.tv/helix/clips";
            var accessTokenObject = _context.TwitchAccessTokens.FirstOrDefault();
            var clientId = _configuration.GetSection("TwitchApi:clientId").Value;

            if (clientId is null)
            {
                throw new Exception("TwitchApi:clientId is empty");
            }
            else if (accessTokenObject is null)
            {
                throw new Exception("Failed to get accessToken");
            }
            else if (accessTokenObject.ExpiresAt < DateTime.Now)
            {
                await _twitchApiService.RefreshTwitchAccessToken();
            }

            string accessToken = accessTokenObject.Value;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Add("Client-Id", clientId);

                string requestUrl = $"{baseUrl}?id={clipId}";
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<TwitchApiClipResponse>(responseBody);
                    return apiResponse!.data;
                }
                else
                {
                    throw new HttpRequestException($"HTTP request failed: {response.StatusCode}, {response.ReasonPhrase}");
                }

            }
        }

        public async Task<ServiceResponse<string>> DeleteClip(int id)
        {
            var response = new ServiceResponse<string>();

            try
            {
                var clip = await _context.Clips
                    .Include(c => c.Comments)
                    .Include(l => l.Likes)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Clip not found";
                    response.Data = "Clip not found";
                    return response;
                }
                _context.Comments.RemoveRange(clip.Comments!);
                _context.Likes.RemoveRange(clip.Likes!);
                _context.Clips.Remove(clip);
                await _context.SaveChangesAsync();

                response.Data = "Clip deleted";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<GetClipDto>> GetClip(int id)
        {
            var response = new ServiceResponse<GetClipDto>();

            try
            {
                var clip = await _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .Include(c => c.Comments!).ThenInclude(u => u.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Clip not found";
                    return response;
                }

                response.Data = _mapper.Map<GetClipDto>(clip);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetClipDto>>> GetClips([FromQuery] QueryObject query)
        {
            var response = new ServiceResponse<List<GetClipDto>>();

            try
            {
                var clips = _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .Include(c => c.Comments).AsQueryable();

                if(!string.IsNullOrWhiteSpace(query.Name))
                    clips = clips.Where(c => c.Title.Contains(query.Name));

                if (!string.IsNullOrWhiteSpace(query.Game))
                    clips = clips.Where(c => c.Game!.Name.Contains(query.Game));

                if (!string.IsNullOrWhiteSpace(query.Broadcaster))
                    clips = clips.Where(c => c.StreamerName.Contains(query.Broadcaster));

                if (query.StartDate is not null)
                    clips = clips.Where(c => c.CreatedAt > query.StartDate);

                if (query.EndDate is not null)
                    clips = clips.Where(c => c.CreatedAt < query.EndDate);

                if (clips.Count() == 0)
                {
                    response.Success = false;
                    response.Message = "No clips found";
                    return response;
                }

                if (!string.IsNullOrWhiteSpace(query.SortBy))
                {
                    if (query.SortBy == "LikeAmount")
                    {
                        clips = query.IsDescending ? clips.OrderByDescending(c => c.LikeAmount)
                            : clips.OrderBy(c => c.LikeAmount);
                    }
                    else if (query.SortBy == "CreatedAt")
                    {
                        clips = query.IsDescending ? clips.OrderByDescending(c => c.CreatedAt)
                            : clips.OrderBy(c => c.CreatedAt);
                    }
                }

                int skipNumber = (query.PageNumber - 1) * query.PageSize;
                clips = clips.Skip(skipNumber).Take(query.PageSize);

                response.Data = await clips.Select(m => _mapper.Map<GetClipDto>(m)).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<GetClipDto>> LikeClip(int id)
        {
            var response = new ServiceResponse<GetClipDto>();

            try
            {
                var clip = await _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .Include(c => c.Comments)
                    .FirstOrDefaultAsync(c => c.Id == id);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Clip not found";
                    return response;
                }
                else if (user is null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                if (await _context.Likes.AnyAsync(l => l.User!.Id == user!.Id && l.Clip!.Id == clip.Id))
                {
                    response.Success = false;
                    response.Message = "Clip already liked by this user";
                    return response;
                }

                var like = new Like
                {
                    User = user,
                    Clip = clip,
                    ClipId = clip.Id
                };

                clip.LikeAmount += 1;
                user.Likes!.Add(like);
                clip.Likes!.Add(like);

                _context.Likes.Add(like);
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

        public async Task<ServiceResponse<GetClipDto>> DislikeClip(int id)
        {
            var response = new ServiceResponse<GetClipDto>();

            try
            {
                var clip = await _context.Clips
                    .Include(u => u.User)
                    .Include(g => g.Game)
                    .Include(c => c.Comments)
                    .FirstOrDefaultAsync(c => c.Id == id);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

                if (clip is null)
                {
                    response.Success = false;
                    response.Message = "Clip not found";
                    return response;
                }
                else if (user is null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                var like = await _context.Likes.FirstOrDefaultAsync(l => l.User!.Id == user!.Id && l.Clip!.Id == clip.Id);

                if (like is null)
                {
                    response.Success = false;
                    response.Message = "Clip not liked by this user";
                    return response;
                }

                clip.Likes!.Remove(like);
                user!.Likes!.Remove(like);
                clip.LikeAmount -= 1;

                _context.Likes.Remove(like);
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

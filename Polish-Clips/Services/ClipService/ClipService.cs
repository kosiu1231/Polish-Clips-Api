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
    }
}

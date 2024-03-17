namespace Polish_Clips.Services.TwitchApiService
{
    public class TwitchApiService : ITwitchApiService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public TwitchApiService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<string>> AddGame(TwitchApiGetGameBy gameByObject)
        {
            var response = new ServiceResponse<string>();

            try
            {
                List<TwitchApiGameObject> games = new List<TwitchApiGameObject>();

                if (gameByObject.Id is null && gameByObject.ByName == false ||
                    string.IsNullOrWhiteSpace(gameByObject.Name) && gameByObject.ByName == true)
                {
                    response.Success = false;
                    response.Message = "Invalid request";
                    return response;
                }

                if (gameByObject.ByName == false)
                {
                    if (await _context.Games.AnyAsync(g => g.Id == gameByObject.Id))
                    {
                        response.Success = false;
                        response.Message = "Game already is in database";
                        return response;
                    }

                    games = await GetGameFromApi(gameByObject);
                }
                else
                {
                    if (await _context.Games.AnyAsync(g => g.Name == gameByObject.Name))
                    {
                        response.Success = false;
                        response.Message = "Game already is in database";
                        return response;
                    }

                    games = await GetGameFromApi(gameByObject);
                }

                if (games.Count() < 1)
                {
                    response.Success = false;
                    response.Message = "Game not found";
                    return response;
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    var game = new Game
                    {
                        Id = games[0].id,
                        Name = games[0].name,
                        ArtUrl = games[0].box_art_url
                    };
                    _context.Games.Add(game);
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [polish-clips].[dbo].[Games] ON;");
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [polish-clips].[dbo].[Games] OFF;");
                    transaction.Commit();
                    response.Data = $"Game {game.Name} added";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<List<TwitchApiGameObject>> GetGameFromApi(TwitchApiGetGameBy gameByObject)
        {
            string baseUrl = "https://api.twitch.tv/helix/games";
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
                await RefreshTwitchAccessToken();
            }

            string accessToken = accessTokenObject.Value;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Add("Client-Id", clientId);

                string requestUrl = "";

                if (gameByObject.ByName == false)
                    requestUrl = $"{baseUrl}?id={gameByObject.Id}";
                else
                    requestUrl = $"{baseUrl}?name={gameByObject.Name}";
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<TwitchApiGameResponse>(responseBody);
                    return apiResponse!.data;
                }
                else
                {
                    throw new HttpRequestException($"HTTP request failed: {response.StatusCode}, {response.ReasonPhrase}");
                }
            }
        }

        public async Task<ServiceResponse<string>> AddClipsByStreamers()
        {
            var response = new ServiceResponse<string>();

            try
            {
                var streamers = await _context.Broadcasters.ToListAsync();
                List<TwitchApiClipObject> clips = new List<TwitchApiClipObject>();
                int clipsAdded = 0;

                foreach (var streamer in streamers)
                {
                    clips = await GetClipsByStreamersFromApi(streamer.TwitchId);

                    if (clips.Count() == 0)
                        continue;

                    foreach (var clip in clips)
                    {
                        if (clip.view_count < 200)
                        {
                            break;
                        }

                        if (await _context.Clips.AnyAsync(c => c.TwitchId == clip.id))
                        {
                            continue;
                        }

                        var newClip = new Clip
                        {
                            TwitchId = clip.id,
                            Title = clip.title,
                            EmbedUrl = clip.embed_url,
                            StreamerName = clip.broadcaster_name,
                            CreatedAt = clip.created_at,
                            ThumbnailUrl = clip.thumbnail_url,
                            Duration = clip.duration
                        };

                        newClip.Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == clip.game_id);

                        if (newClip.Game is null)
                        {
                            TwitchApiGetGameBy gameByObject = new TwitchApiGetGameBy
                            {
                                Id = clip.game_id,
                                ByName = false
                            };

                            await AddGame(gameByObject);
                            newClip.Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == clips[0].game_id);
                        }

                        _context.Clips.Add(newClip);
                        newClip.Game!.Clips!.Add(newClip);
                        clipsAdded++;
                    }
                }

                await _context.SaveChangesAsync();
                response.Data = $"{clipsAdded} clips added to DB";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;

            }

            return response;
        }

        public async Task<List<TwitchApiClipObject>> GetClipsByStreamersFromApi(string streamerId)
        {
            string baseUrl = "https://api.twitch.tv/helix/clips";
            var accessTokenObject = _context.TwitchAccessTokens.FirstOrDefault();
            var clientId = _configuration.GetSection("TwitchApi:clientId").Value;
            DateTime startDate = DateTime.UtcNow.AddHours(-8);
            DateTime endDate = DateTime.UtcNow;
            string startDateRFC = startDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
            string endDateRFC = endDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

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
                await RefreshTwitchAccessToken();
            }

            string accessToken = accessTokenObject.Value;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Add("Client-Id", clientId);

                string requestUrl = $"{baseUrl}?broadcaster_id={streamerId}&started_at={startDateRFC}&ended_at={endDateRFC}";
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
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

        public async Task<ServiceResponse<string>> AddBroadcasters()
        {
            var response = new ServiceResponse<string>();

            try
            {
                List<TwitchApiBroadcasterObject> broadcasters = new List<TwitchApiBroadcasterObject>();
                int broadcastersAdded = 0;

                broadcasters = await GetBroadcastersFromApi();

                if (broadcasters.Count() < 1)
                {
                    response.Success = false;
                    response.Message = "Broadcasters not found";
                    return response;
                }

                foreach (var broadcaster in broadcasters)
                {
                    if (broadcaster.viewer_count < 2000)
                    {
                        break;
                    }

                    if (await _context.Broadcasters.AnyAsync(b => b.TwitchId == broadcaster.user_id))
                    {
                        continue;
                    }

                    var newBroadcaster = new Broadcaster
                    {
                        TwitchId = broadcaster.user_id,
                        Name = broadcaster.user_name
                    };

                    _context.Broadcasters.Add(newBroadcaster);
                    broadcastersAdded++;
                }

                await _context.SaveChangesAsync();
                response.Data = $"{broadcastersAdded} broadcasters added to DB";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<List<TwitchApiBroadcasterObject>> GetBroadcastersFromApi()
        {
            string baseUrl = "https://api.twitch.tv/helix/streams";
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
                await RefreshTwitchAccessToken();
            }

            string accessToken = accessTokenObject.Value;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Add("Client-Id", clientId);

                string requestUrl = $"{baseUrl}?language=pl";
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<TwitchApiBroadcasterResponse>(responseBody);
                    return apiResponse!.data;
                }
                else
                {
                    throw new HttpRequestException($"HTTP request failed: {response.StatusCode}, {response.ReasonPhrase}");
                }
            }
        }

        public async Task<ServiceResponse<string>> RefreshTwitchAccessToken()
        {
            var response = new ServiceResponse<string>();

            try
            {
                var accessToken = _context.TwitchAccessTokens.FirstOrDefault();
                string baseUrl = "https://id.twitch.tv/oauth2/token";
                var clientId = _configuration.GetSection("TwitchApi:clientId").Value;
                var clientSecret = _configuration.GetSection("TwitchApi:clientSecret").Value;
                string grantType = "client_credentials";

                if (clientId is null)
                {
                    throw new Exception("TwitchApi:clientId is empty");
                }
                else if (accessToken is null)
                {
                    throw new Exception("Failed to get accessToken");
                }
                else if (clientSecret is null)
                {
                    throw new Exception("TwitchApi:clientSecret is empty");
                }

                using (HttpClient client = new HttpClient())
                {
                    var parameters = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", grantType }
                };

                    var content = new FormUrlEncodedContent(parameters);
                    HttpResponseMessage apiResponse = await client.PostAsync(baseUrl, content);

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        string responseBody = await apiResponse.Content.ReadAsStringAsync();
                        var newAccessToken = JsonConvert.DeserializeObject<TwitchApiAccessTokenObject>(responseBody);
                        accessToken.Value = newAccessToken!.access_token;
                        accessToken.ExpiresAt = DateTime.Now.AddMinutes((newAccessToken.expires_in / 60) - 5);
                        await _context.SaveChangesAsync();
                        response.Data = $"Token refreshed";
                    }
                    else
                    {
                        throw new HttpRequestException($"HTTP request failed: {apiResponse.StatusCode}, {apiResponse.ReasonPhrase}");
                    }
                }
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

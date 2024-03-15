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

                    games = await GetGame(gameByObject);
                }
                else
                {
                    if (await _context.Games.AnyAsync(g => g.Name == gameByObject.Name))
                    {
                        response.Success = false;
                        response.Message = "Game already is in database";
                        return response;
                    }

                    games = await GetGame(gameByObject);
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

        public async Task<List<TwitchApiGameObject>> GetGame(TwitchApiGetGameBy gameByObject)
        {
            string baseUrl = "https://api.twitch.tv/helix/games";
            string accessToken = "wctrbwdc88lix37nrud4o7bld4uviu";
            var clientId = _configuration.GetSection("TwitchApi:clientId").Value;

            if (clientId is null)
            {
                throw new Exception("TwitchApi:clientId is empty");
            }
            else if (accessToken is null)
            {
                throw new Exception("Failed to get accessToken");
            }

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
    }
}

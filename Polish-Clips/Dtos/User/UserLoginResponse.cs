namespace Polish_Clips.Dtos.User
{
    public class UserLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public GetUserDto? User { get; set; }
        public List<GetLikeDto>? Likes { get; set; }
    }
}

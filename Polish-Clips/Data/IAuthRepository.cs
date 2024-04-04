namespace Polish_Clips.Data
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<string>> Register(User user, string password, string confirmPassword);
        Task<ServiceResponse<UserLoginResponse>> Login(string email, string password);
        Task<ServiceResponse<string>> Verify(string token);
        Task<ServiceResponse<string>> ForgotPassword(string email);
        Task<ServiceResponse<string>> ResetPassword(ResetPasswordRequest request);
        Task<bool> UsernameExists(string username);
        Task<bool> EmailExists(string email);
        Task<string> CreateRandomToken();
    }
}

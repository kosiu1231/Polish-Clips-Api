namespace Polish_Clips.Dtos.User
{
    public class UserRegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(4, ErrorMessage = "Username has to be at least 4 characters")]
        [MaxLength(32, ErrorMessage = "Username has to be 32 characters at maximum")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Password has to be at least 6 characters")]
        [MaxLength(32, ErrorMessage = "Password has to be 32 characters at maximum")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

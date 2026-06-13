namespace PRN232.LMS.Services.Interfaces
{
    public class LoginResponse
    {
        public string AccessToken { get; internal set; }
        public string RefreshToken { get; internal set; }
        public int ExpiresIn { get; internal set; }
    }
}
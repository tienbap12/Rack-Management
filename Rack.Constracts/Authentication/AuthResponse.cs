namespace Rack.Contracts.Authentication
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
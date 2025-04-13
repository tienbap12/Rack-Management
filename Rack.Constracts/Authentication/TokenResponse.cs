namespace Rack.Contracts.Authentication.Response;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
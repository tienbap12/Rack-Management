namespace Rack.Contracts.Authentication;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}
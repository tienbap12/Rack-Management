﻿namespace Rack.Contracts.Authentication
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; }
    }
}

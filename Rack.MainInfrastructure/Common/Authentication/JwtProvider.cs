using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using Rack.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Rack.MainInfrastructure.Common.Authentication
{
    internal class JwtProvider(IOptions<JwtSettings> options) : IJwtProvider
    {
        private readonly JwtSettings _jwtSettings = options.Value;

        public string Generate(Account req, string roleName)
        {
            var secretKey = _jwtSettings.SecurityKey;
            var tokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes);
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(secretKey);
            var authClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Name, req.FullName),
            new Claim(JwtRegisteredClaimNames.UniqueName, req.Username),
            new Claim(ClaimTypes.Role, roleName),
        };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: tokenExpires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature));
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32]; // Giảm kích thước từ 64 -> 32 byte (đủ an toàn)
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Base64UrlEncoder.Encode(randomNumber);
        }
    }
}
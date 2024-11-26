using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

public class JwtTokenHelper
{
    private readonly IConfiguration _configuration;

    public JwtTokenHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(string username)
    {
        // Check if secretKey is available, otherwise use a default value
        var secretKey = _configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            secretKey = "692a822cd595fc5125b2da4e54241eebeac8a1b178be78380b05daef4864436e"; // Use a default secret key if not found in config
        }

        var issuer = _configuration["JwtSettings:Issuer"];
        if (string.IsNullOrEmpty(issuer))
        {
            issuer = "default-issuer"; // Use a default issuer if not found
        }

        var audience = _configuration["JwtSettings:Audience"];
        if (string.IsNullOrEmpty(audience))
        {
            audience = "default-audience"; // Use a default audience if not found
        }
        
        // Safely parse expiration minutes, using a default value if parsing fails
        var expirationMinutesString = _configuration["JwtSettings:ExpirationMinutes"];
        int expirationMinutes = 60; // Default value in minutes
        if (!int.TryParse(expirationMinutesString, out expirationMinutes))
        {
            expirationMinutes = 60; // If parsing fails, fallback to default value
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.Now.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

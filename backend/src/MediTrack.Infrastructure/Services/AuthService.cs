using MediTrack.Application.Dtos.Auth;
using MediTrack.Application.Interfaces;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MediTrack.Domain.Exceptions.Authentication;
using BCryptNet = BCrypt.Net.BCrypt; // Alias to avoid naming conflicts

namespace MediTrack.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto registerDto)
    {
        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null)
        {
            throw new DuplicateEmailException();
        }

        // Hash the password
        string passwordHash = BCryptNet.HashPassword(registerDto.Password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Optionally create a UserProfile here or leave it for later update
        // var userProfile = new UserProfile { UserId = newUser.Id, /* other defaults */ };
        // _context.UserProfiles.Add(userProfile);
        // await _context.SaveChangesAsync();

        // Generate token for the new user
        return GenerateJwtToken(newUser);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
        {
            return null; // User not found
        }

        // Verify password
        bool isPasswordValid = BCryptNet.Verify(loginDto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null; // Invalid password
        }

        // Generate token
        return GenerateJwtToken(user);
    }

    private AuthResponseDto GenerateJwtToken(User user)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            
            // Validate JWT settings
            var key = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            var expirationHours = jwtSettings["ExpirationHours"] ?? throw new InvalidOperationException("JWT ExpirationHours not configured");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            if (securityKey.KeySize < 256)
            {
                throw new InvalidOperationException("JWT Key must be at least 32 characters long");
            }

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = DateTime.UtcNow.AddHours(Convert.ToDouble(expirationHours));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            if (string.IsNullOrEmpty(tokenString))
            {
                throw new InvalidOperationException("Generated token string is null or empty");
            }

            return new AuthResponseDto
            {
                Token = tokenString,
                Expiration = expires,
                UserId = user.Id,
                Email = user.Email
            };
        }
        catch (Exception ex)
        {
            // Log the specific error
            throw new InvalidOperationException($"Error generating JWT token: {ex.Message}", ex);
        }
    }
}

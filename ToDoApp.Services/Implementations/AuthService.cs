using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ToDoApp.DataAccess.Context;
using ToDoApp.DataAccess.Entities;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ToDoDbContext _context;

    public AuthService(IConfiguration configuration, ToDoDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task<string> LoginWithGoogleAsync(string googleToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}

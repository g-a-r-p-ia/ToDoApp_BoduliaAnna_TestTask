using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ToDoApp.Interfaces.Entities;
using ToDoApp.Interfaces.Repositories;
using ToDoApp.Services.DTOs;
using ToDoApp.Services.Interfaces;

namespace ToDoApp.Services.Implementations;

public class AuthService : IAuthService
{
    private const int PasswordSaltSize = 16;
    private const int PasswordHashSize = 32;
    private const int PasswordIterations = 100_000;

    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AuthService(IConfiguration configuration, IUserRepository userRepository, ICategoryRepository categoryRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<string> LoginWithGoogleAsync(string googleToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);

        var user = await _userRepository.GetByEmailAsync(payload.Email);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName
            };

            user.Categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Work" },
                new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Personal" },
                new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Study" }
            };

            await _userRepository.AddAsync(user);
        }
        else
        {
            await SeedDefaultCategoriesIfMissingAsync(user.Id);
        }

        return GenerateToken(user);
    }

    private async Task SeedDefaultCategoriesIfMissingAsync(Guid userId)
    {
        var existingCategories = await _categoryRepository.GetForUserAsync(userId);

        if (existingCategories.Any())
        {
            return;
        }

        foreach (var categoryName in new[] { "Work", "Personal", "Study" })
        {
            await _categoryRepository.AddAsync(new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = categoryName
            });
        }
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = HashPassword(dto.Password)
        };

        user.Categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Work" },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Personal" },
            new Category { Id = Guid.NewGuid(), UserId = user.Id, Name = "Study" }
        };

        await _userRepository.AddAsync(user);

        return GenerateToken(user);
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
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

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(PasswordSaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, PasswordIterations, HashAlgorithmName.SHA256, PasswordHashSize);

        return $"{PasswordIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

using BlazorBattles.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<string>> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return FailedLoginResponse();
            }

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PassswordSalt))
            {
                return FailedLoginResponse();
            }

            var response = new ServiceResponse<string>()
            {
                IsSuccess = true,
                Message = "Login sucessful!",
                Data = CreateToken(user)
            };
            return response;
        }

        private static ServiceResponse<string> FailedLoginResponse()
        {
            var response = new ServiceResponse<string>();
            response.IsSuccess = false;
            response.Message = "Login failed.";

            return response;
        }

        public async Task<ServiceResponse<int>> RegisterAsync(User user, string password)
        {
            if (await UserExistsAsync(user.Email))
            {
                return new ServiceResponse<int> { IsSuccess = false, Message = "User already exists." };
            }

            CreateEncryptedPassword(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PassswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new ServiceResponse<int> { Data = user.Id, IsSuccess = true, Message = "Registration sucessful!" };
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            if (await _context.Users.AnyAsync(user => user.Email.ToLower() == (email.ToLower())))
            {
                return true;
            }
            return false;
        }

        private void CreateEncryptedPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string CreateToken(User user)
        {
            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            string symmetricKey = _configuration.GetSection("TokenSettings:SymmetricKey").Value;
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(symmetricKey));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}

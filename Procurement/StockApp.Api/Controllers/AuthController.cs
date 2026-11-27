using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockApp.Api.Services;
using StockApp.Data;
using StockApp.Data.Entities;
using StockApp.Shared.Dtos.Auth;

namespace StockApp.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AppDbContext db, TokenService tokens, IPasswordHasher<UserAuth> hasher) : ControllerBase
    {
        private const int MaxFailed = 5;
        private static readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(15);

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var userName = request.Username.Trim();
            var user = await db.UserAuths.Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            string? failReason = null;
            var rehash = false;

            if (user is null) failReason = "Username tidak ditemukan";
            else if (!user.IsActive || !user.Employee.IsActive) failReason = "Akun nonaktif";
            else if (user.LockoutEnd > DateTime.UtcNow) failReason = "Akun terkunci";
            else
            {
                var check = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (check == PasswordVerificationResult.Failed)
                {
                    failReason = "Password salah";
                    user.FailedCount++;
                    if (user.FailedCount >= MaxFailed)
                    {
                        user.LockoutEnd = DateTime.UtcNow.Add(LockoutTime);
                        user.FailedCount = 0;
                    }
                }
                else rehash = check == PasswordVerificationResult.SuccessRehashNeeded;
            }

            if (failReason is null && user is not null)
            {
                user.FailedCount = 0;
                user.LockoutEnd = null;
                user.LastLoginAt = DateTime.UtcNow;
                if (rehash) user.PasswordHash = hasher.HashPassword(user, request.Password);
            }

            await WriteLogAsync(userName, user?.Id.ToString(), failReason);   // sekaligus menyimpan perubahan user

            if (failReason is not null)
            {
                // HTTP 200 + Success=false supaya tidak muncul error merah di console browser.
                return new LoginResponse
                {
                    Success = false,
                    Message = failReason == "Akun terkunci"
                        ? "Akun terkunci sementara. Coba lagi beberapa menit lagi."
                        : "Username atau password salah.",   // sengaja seragam, tidak membocorkan penyebab
                };
            }

            var (token, expires) = tokens.Create(user!, RolesOf(user!));

            return new LoginResponse
            {
                Success = true,
                Token = token,
                ExpiresAt = expires,
                UserName = user!.UserName,
                FullName = user.Employee.Name,
                Roles = RolesOf(user),
            };
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<LoginResponse>> Me()
        {
            if (!int.TryParse(User.FindFirst("sub")?.Value, out var id)) return Unauthorized();

            var user = await db.UserAuths.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == id);
            if (user is null || !user.IsActive || !user.Employee.IsActive) return Unauthorized();

            return new LoginResponse
            {
                Success = true,
                UserName = user.UserName,
                FullName = user.Employee.Name,
                Roles = RolesOf(user),
            };
        }

        // Role akun = Employees.Role
        private static string[] RolesOf(UserAuth user) =>
            string.IsNullOrWhiteSpace(user.Employee.Role) ? [] : [user.Employee.Role];

        private async Task WriteLogAsync(string userName, string? userId, string? failReason)
        {
            var agent = Request.Headers.UserAgent.ToString();

            db.LoginLogs.Add(new LoginLog
            {
                UserName = userName.Length > 256 ? userName[..256] : userName,
                UserId = userId,
                Success = failReason is null,
                FailReason = failReason,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = agent.Length > 512 ? agent[..512] : agent,
            });
            await db.SaveChangesAsync();
        }
    }
}
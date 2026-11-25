using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockApp.Api.Services;
using StockApp.Data;
using StockApp.Data.Entities;
using StockApp.Shared.Dtos.Auth;

namespace StockApp.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(UserManager<AppUser> users, AppDbContext db, TokenService tokens) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var userName = request.Username.Trim();
            var user = await users.FindByNameAsync(userName);

            string? failReason = null;
            if (user is null) failReason = "Username tidak ditemukan";
            else if (!user.IsActive) failReason = "Akun nonaktif";
            else if (await users.IsLockedOutAsync(user)) failReason = "Akun terkunci";
            else if (!await users.CheckPasswordAsync(user, request.Password))
            {
                await users.AccessFailedAsync(user);   // menambah hitungan gagal; terkunci setelah 5x
                failReason = "Password salah";
            }

            await WriteLogAsync(userName, user?.Id, failReason);

            if (failReason is not null)
            {
                var message = failReason == "Akun terkunci"
                    ? "Akun terkunci sementara. Coba lagi beberapa menit lagi."
                    : "Username atau password salah.";   // sengaja seragam, tidak membocorkan penyebab
                return Unauthorized(new { message });
            }

            await users.ResetAccessFailedCountAsync(user!);

            var roles = await users.GetRolesAsync(user!);
            var (token, expires) = tokens.Create(user!, roles);

            return new LoginResponse
            {
                Token = token,
                ExpiresAt = expires,
                UserName = user!.UserName!,
                FullName = user.FullName,
                Roles = roles.ToArray(),
            };
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<LoginResponse>> Me()
        {
            var user = await users.FindByIdAsync(User.FindFirst("sub")?.Value ?? "");
            if (user is null || !user.IsActive) return Unauthorized();

            return new LoginResponse
            {
                UserName = user.UserName!,
                FullName = user.FullName,
                Roles = (await users.GetRolesAsync(user)).ToArray(),
            };
        }

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
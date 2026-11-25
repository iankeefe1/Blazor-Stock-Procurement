using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace StockApp.Web.Services.Auth
{
    public class ApiAuthStateProvider(TokenStore store) : AuthenticationStateProvider
    {
        private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await store.GetAsync();
            if (string.IsNullOrWhiteSpace(token)) return Anonymous;

            var claims = ParseClaims(token);
            if (claims is null)            // token rusak atau sudah kedaluwarsa
            {
                await store.ClearAsync();
                return Anonymous;
            }

            var identity = new ClaimsIdentity(claims, "jwt", "name", "role");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void NotifyChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        private static List<Claim>? ParseClaims(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length != 3) return null;

                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

                using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
                var root = doc.RootElement;

                if (root.TryGetProperty("exp", out var exp) &&
                    DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow)
                    return null;

                var claims = new List<Claim>();
                foreach (var p in root.EnumerateObject())
                {
                    if (p.Value.ValueKind == JsonValueKind.Array)
                        foreach (var v in p.Value.EnumerateArray())
                            claims.Add(new Claim(p.Name, v.ToString()));
                    else
                        claims.Add(new Claim(p.Name, p.Value.ToString()));
                }
                return claims;
            }
            catch
            {
                return null;
            }
        }
    }
}
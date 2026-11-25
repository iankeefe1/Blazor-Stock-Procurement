namespace StockApp.Shared.Dtos.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string[] Roles { get; set; } = [];
    }
}
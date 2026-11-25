using System.Net;
using System.Net.Http.Json;

namespace StockApp.Web.Services
{
    internal record ErrorBody(string? Message);

    public static class ApiError
    {
        // Mengubah respons gagal dari API menjadi pesan yang bisa ditampilkan.
        public static async Task<string> ReadAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return "Anda tidak punya izin untuk aksi ini.";

            try
            {
                var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
                if (!string.IsNullOrWhiteSpace(body?.Message)) return body.Message;
            }
            catch { /* body bukan JSON */ }

            return response.StatusCode == HttpStatusCode.BadRequest
                ? "Data tidak valid."
                : $"Terjadi kesalahan (kode {(int)response.StatusCode}).";
        }
    }
}
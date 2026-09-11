using System.Net.Http.Headers;
using System.Text.Json;
using SixLabors.ImageSharp;
namespace CatalogStudio.Services;

public class OpenAiImageService(HttpClient http, IConfiguration config, FileService files, ILogger<OpenAiImageService> logger)
{
    string Key => !string.IsNullOrWhiteSpace(config["OPENAI_API_KEY"])?config["OPENAI_API_KEY"]!:!string.IsNullOrWhiteSpace(config["OpenAI:ApiKey"])?config["OpenAI:ApiKey"]!:throw new InvalidOperationException("OpenAI API yapılandırması bulunamadı.");
    public async Task<string> EditAsync(string name, string prompt, CancellationToken ct, string size = "1024x1536")
    {
        // Retry only explicit rate limits; uncertain failures may already have incurred a charge.
        for (int attempt = 0; attempt < 3; attempt++)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "images/edits"); req.Headers.Authorization = new("Bearer", Key);
            using var body = new MultipartFormDataContent(); body.Add(new StringContent(config["OpenAI:ImageModel"] ?? "gpt-image-2"), "model"); body.Add(new StringContent(prompt), "prompt"); body.Add(new StringContent(size), "size"); body.Add(new StringContent(config["OpenAI:Quality"] ?? "medium"), "quality"); body.Add(new StringContent("png"), "output_format");
            var image = new ByteArrayContent(await File.ReadAllBytesAsync(files.PathFor(name), ct)); image.Headers.ContentType = new MediaTypeHeaderValue("image/png"); body.Add(image, "image[]", "reference.png"); req.Content = body;
            using var response = await http.SendAsync(req, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < 2) { await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(response.Headers.RetryAfter?.Delta?.TotalSeconds ?? (2 << attempt), 1, 30)), ct); continue; }
            logger.LogInformation("OpenAI image edit completed with HTTP {StatusCode}.",(int)response.StatusCode); response.EnsureSuccessStatusCode(); using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct)); var encoded = json.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString(); if (string.IsNullOrEmpty(encoded)) throw new InvalidOperationException("Görsel yanıtı boş."); var bytes = Convert.FromBase64String(encoded); using var decoded = Image.Load(bytes); var path = files.NewPath(temporary:true); await decoded.SaveAsPngAsync(path, ct); return Path.GetFileName(path);
        }
        throw new InvalidOperationException("Görsel servisi şu anda yoğun.");
    }
}
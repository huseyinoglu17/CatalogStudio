using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace CatalogStudio.Services;

public class FileService(IStoragePathService paths)
{
    public string Root { get; } = paths.FilesPath;
    public string PathFor(string name) { if (Path.GetFileName(name) != name || string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Geçersiz dosya."); return Path.Combine(name.StartsWith("tmp_",StringComparison.Ordinal)?paths.TempPath:Root, name); }
    public string NewPath(bool temporary=false) { Directory.CreateDirectory(temporary?paths.TempPath:Root); return PathFor((temporary?"tmp_":"")+Guid.NewGuid().ToString("N") + ".png"); }
    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct)
    {
        if (!new[]{".png",".jpg",".jpeg",".webp"}.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())) throw new InvalidOperationException("PNG, JPEG veya WebP dosyası yükleyin.");
        if (file.Length is <= 0 or > 10 * 1024 * 1024 || !new[] { "image/png", "image/jpeg", "image/webp" }.Contains(file.ContentType)) throw new InvalidOperationException("PNG, JPEG veya WebP yükleyin (en fazla 10 MB).");
        await using var stream = file.OpenReadStream();
        try { var info = await Image.IdentifyAsync(stream, ct); if (info.Width * (long)info.Height > 24000000) throw new InvalidOperationException("Görsel en fazla 24 megapiksel olabilir."); stream.Position = 0; using var img = await Image.LoadAsync(stream, ct); img.Mutate(x => x.AutoOrient()); img.Metadata.ExifProfile = null; var path = NewPath(); await img.SaveAsPngAsync(path, ct); return Path.GetFileName(path); } catch (UnknownImageFormatException) { throw new InvalidOperationException("Dosya geçerli bir görsel değil."); }
    }
    public void DeleteCatalog(CatalogStudio.Models.Catalog c){ Delete(c.GeneratedCatalogPath); Delete(c.MainProductImagePath); Delete(c.LogoSnapshotPath); foreach(var name in System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.VariantPathsJson)??[]) Delete(name); }
    public void Delete(string? name) { if (!string.IsNullOrEmpty(name)) File.Delete(PathFor(name)); }
}

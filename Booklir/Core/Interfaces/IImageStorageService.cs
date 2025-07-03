namespace Booklir.Core.Interfaces
{
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string subFolder);
        Task DeleteImageAsync(string imageUrl);
    }
}

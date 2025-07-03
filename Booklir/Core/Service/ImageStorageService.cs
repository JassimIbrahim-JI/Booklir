using Booklir.Core.Interfaces;

namespace Booklir.Core.Service
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _env;
        public ImageStorageService(IWebHostEnvironment environment)
        {
            _env = environment;
        }
        public Task DeleteImageAsync(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string subFolder)
        {
            // subFolder = "profile" or "trips" or "destination"
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder.Trim());
          var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            Directory.CreateDirectory(uploadsFolder);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}

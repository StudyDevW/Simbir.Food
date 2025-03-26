using ORM_Components.DTO.RestaurantAPI;
using RestaurantAPI.Interface;

namespace RestaurantAPI.Services;

public class FileSystemHandler : IFileSystemHandler
{
    private const string FOLDER_NAME = "uploads";

    public async Task<string> AddPhoto(IFormFile file)
    {
        var uploads = Path.GetFullPath(FOLDER_NAME);
        var filePath = Path.Combine(uploads, file.FileName);

        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        return filePath;
    }

    public void DeletePhoto(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}

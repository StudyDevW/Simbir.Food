namespace RestaurantAPI.Interface;

public interface IFileSystemHandler
{
    Task<string> AddPhoto(IFormFile file);
    void DeletePhoto(string path);
}

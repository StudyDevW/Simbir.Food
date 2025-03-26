using Microsoft.AspNetCore.Http;
using RestaurantAPI.Interface;

namespace TestsBaseLib.Mocks;

public class InnerFileSystemHandlerMock : IFileSystemHandler
{

    public Task<string> AddPhoto(IFormFile file)
    {
        string path = "VirtualDisk:\\Photos\\" + file.FileName;
        return Task.FromResult(path);
    }

    public void DeletePhoto(string path) { }
}

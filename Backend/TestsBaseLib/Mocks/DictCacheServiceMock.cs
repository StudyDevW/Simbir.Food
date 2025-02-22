using Middleware_Components.Services;
using System.Text.Json;

namespace TestsBaseLib.Mocks;

public class DictCacheServiceMock : ICacheService
{
    private readonly Dictionary<string, string> _dict;

    public IReadOnlyDictionary<string, string> Dict
    {
        get => _dict;
    }

    public DictCacheServiceMock()
    {
        _dict = new Dictionary<string, string>();
    }

    public bool CheckExistKeysStorage<T>(Guid id_user, string type)
    {
        var cache_data = GetData<T>($"{type}_storage_{id_user}");

        if (cache_data != null)
            return true;

        return false;
    }

    public bool CheckExistKeysStorage(Guid id_user, string type)
    {
        var cache_data = GetData<string>($"{type}_storage_{id_user}");

        if (cache_data != null)
            return true;

        return false;
    }

    public bool CheckExistKeysStorage<T>(string storage_desc)
    {
        var cache_data = GetData<T>($"{storage_desc}_storage");

        if (cache_data != null)
            return true;

        return false;
    }

    public void DeleteKeyFromStorage(Guid id_user, string type)
    {
        RemoveData($"{type}_storage_{id_user}");
    }

    public void DeleteKeyFromStorage(string storage_desc)
    {
        RemoveData($"{storage_desc}_storage");
    }

    public T GetData<T>(string key)
    {
        var result = _dict.TryGetValue(key, out var data);

        if (!result)
            return default;

        return JsonSerializer.Deserialize<T>(data);
    }

    public string? GetKeyFromStorage(Guid id_user, string type)
    {
        var cache_data = GetData<string>($"{type}_storage_{id_user}");

        return cache_data;
    }

    public T GetKeyFromStorage<T>(Guid id_user, string type)
    {
        var cache_data = GetData<T>($"{type}_storage_{id_user}");

        return cache_data;
    }

    public T GetKeyFromStorage<T>(string storage_desc)
    {
        var cache_data = GetData<T>($"{storage_desc}_storage");

        return cache_data;
    }

    public object RemoveData(string key)
    {
        var exists = _dict.TryGetValue(key, out var _);

        if (exists)
            _dict.Remove(key);

        return exists;
    }

    public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
    {
        _dict[key] = JsonSerializer.Serialize(value);
        return true;
    }

    public void WriteKeyInStorage(Guid id_user, string type, string key, DateTime extime)
    {
        SetData($"{type}_storage_{id_user}", key, extime);
    }

    public void WriteKeyInStorage<T>(Guid id_user, string type, T key, DateTime extime)
    {
        SetData($"{type}_storage_{id_user}", key, extime);
    }

    public void WriteKeyInStorageObject<T>(string storage_desc, T key, DateTime extime)
    {
        SetData($"{storage_desc}_storage", key, extime);
    }
}

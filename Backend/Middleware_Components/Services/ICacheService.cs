using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.Services
{
    public interface ICacheService
    {
        public T GetData<T>(string key);

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime);

        public object RemoveData(string key);

        public void WriteKeyInStorageObject<T>(string storage_desc, T key, DateTime extime);

        public void WriteKeyInStorage(int id_user, string type, string key, DateTime extime);

        public void WriteKeyInStorage<T>(int id_user, string type, T key, DateTime extime);

        public void DeleteKeyFromStorage(int id_user, string type);

        public void DeleteKeyFromStorage(string storage_desc);

        public bool CheckExistKeysStorage<T>(int id_user, string type);

        public bool CheckExistKeysStorage(int id_user, string type);

        public bool CheckExistKeysStorage<T>(string storage_desc);

        public string? GetKeyFromStorage(int id_user, string type);

        public T GetKeyFromStorage<T>(int id_user, string type);

        public T GetKeyFromStorage<T>(string storage_desc);
    }
}

using Pipelines.Sockets.Unofficial.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.ClientAPI
{

    /// <summary>
    ///  Переменные взяты из API Mini Apps VK
    ///  https://dev.vk.com/ru/bridge/VKWebAppGetUserInfo
    /// </summary>
    public class AuthAddUser 
    {
        public long vk_id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string? photo_max_orig { get; set; }

        public string? address { get; set; }

        public string device { get; set; }

        public string[]? roles { get; set; }
    }
}

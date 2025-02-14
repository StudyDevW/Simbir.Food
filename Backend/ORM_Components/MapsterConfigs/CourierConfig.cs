using Mapster;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.MapsterConfigs
{
    public static class CourierConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<CourierDtoForCreate, CourierTable>.NewConfig();
        }
    }
}

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
            TypeAdapterConfig<CourierDtoForCreate, CourierTable>.NewConfig()
                .Map(dest => dest.userId, src => src.userId)
                .Map(dest => dest.car_number, src => src.car_number);

            TypeAdapterConfig<CourierTable, CourierDto>.NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.userId, src => src.userId)
                .Map(dest => dest.car_number, src => src.car_number)
                .Map(dest => dest.status, src => src.status);
        }
    }
}

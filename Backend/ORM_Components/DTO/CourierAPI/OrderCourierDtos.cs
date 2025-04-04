using ORM_Components.Tables.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.CourierAPI
{
    public record OrderForCourierDtoShort(Guid orderId, Guid restaurantId, Guid clientId, OrderStatus status, DateTime orderDate) { }
    public record OrderForCourierDto(Guid orderId, Guid restaurantId, string restaurantName, string restaurantAddress, string clientAddress, DateTime orderDate) { }
}

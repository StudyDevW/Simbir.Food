using ORM_Components.Tables.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.CourierAPI
{
    public record OrderForCourierDto(Guid orderId, Guid restaurantId, OrderStatus status, DateTime orderDate) { }
    public record OrderLinkCourierDto(Guid orderId, Guid courierId) { }
}

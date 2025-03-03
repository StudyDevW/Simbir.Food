using ORM_Components.Tables.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.DTO.RestaurantAPI
{
    public record Restaurants_DTO(
         Guid Id, Guid userId,
         string restaurantName, string address,
         string phone_number, RestaurantStatus status,
         string description, string imagePath,
         string open_time, string close_time)
    { }

    public record RestaurantCreate_DTO(
        Guid userId, string restaurantName,
        string address, string phone_number,
        string description, string imagePath,
        string open_time, string close_time)
    { }

    public record RestaurantUpdate_DTO(
        string? restaurantName, string? address,
        string? phone_number, RestaurantStatus? status,
        string? description, string? imagePath,
        string? open_time, string? close_time)
    { }

    public record RestaurantMark_DTO(
        Guid Id, string restaurantName,
        float averageMark)
    { }

}

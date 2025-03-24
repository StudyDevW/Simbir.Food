namespace ORM_Components.DTO.ClientAPI.Review
{
    public record ReviewDtoForUpdate(int? rating, string? comment) { }

    public record ReviewDto(
        Guid reviewId, Guid order_id, 
        Guid client_id, Guid courier_id,
        int rating, string comment,
        DateTime review_date) { }
}

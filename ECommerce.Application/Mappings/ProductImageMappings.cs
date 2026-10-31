using ECommerce.Application.DTOs.Products;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public static class ProductImageMappings
{
    public static ProductImageDto ToDto(this ProductImage i) => new(i.Id, i.Url, i.IsMain);
}
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Common.Caching;

public static class CacheTags
{
    public const string Products = "products";
    public const string Categories = "categories";
    public const string Brands = "brands";
}

public static class CacheKeys
{
    public const string Categories = "categories:all";
    public const string Brands = "brands:all";

    public static string Product(int id) => $"products:{id}";

    public static string ProductList(
        int page, int size, string? search, int? categoryId, int? brandId)
        => $"products:list:{page}:{size}:{Uri.EscapeDataString(search ?? "")}:{categoryId}:{brandId}";
}
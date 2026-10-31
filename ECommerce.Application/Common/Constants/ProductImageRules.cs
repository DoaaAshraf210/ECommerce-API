namespace ECommerce.Application.Common.Constants;

public static class ProductImageRules
{
    public const long MaxFileBytes = 5 * 1024 * 1024;
    public const int MaxFilesPerRequest = 5;
    public const int MaxImagesPerProduct = 8;
}
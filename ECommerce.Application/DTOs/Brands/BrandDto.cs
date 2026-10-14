public record BrandDto(int Id, string Name, string? LogoUrl);
public record CreateBrandDto(string Name, string? LogoUrl);
public record UpdateBrandDto(string Name, string? LogoUrl);
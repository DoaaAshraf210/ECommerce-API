public record CategoryDto(int Id, string Name, string? Description, string? ImageUrl);
public record CreateCategoryDto(string Name, string? Description, string? ImageUrl);
public record UpdateCategoryDto(string Name, string? Description, string? ImageUrl);
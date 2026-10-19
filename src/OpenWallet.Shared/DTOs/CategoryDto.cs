namespace OpenWallet.Shared.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
    public string ParentCategoryName { get; set; } = string.Empty;
    public List<CategoryDto> SubCategories { get; set; } = [];
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
}

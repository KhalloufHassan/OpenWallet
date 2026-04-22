namespace OpenWallet.Database.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; } = default!;
    public List<Category> SubCategories { get; set; } = [];
    public List<Record> Records { get; set; } = [];
}
